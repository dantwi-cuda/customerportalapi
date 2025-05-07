using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CCICustomerPortalApi.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ITenantService _tenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<CCICustomerPortalApi.Models.Program> Programs { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportCategory> ReportCategories { get; set; }
    public DbSet<ShopKpi> ShopKpis { get; set; }
    public DbSet<CustomerUser> CustomerUsers { get; set; }
    public DbSet<UserWorkspace> UserWorkspaces { get; set; }
    public DbSet<ShopProgram> ShopPrograms { get; set; }
    public DbSet<ShopUser> ShopUsers { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<CustomerShop> CustomerShops { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure many-to-many relationships
        builder.Entity<CustomerUser>()
            .HasKey(cu => new { cu.CustomerId, cu.UserId });

        builder.Entity<UserWorkspace>()
            .HasKey(uw => new { uw.UserId, uw.WorkspaceId });

        builder.Entity<ShopProgram>()
            .HasKey(sp => new { sp.ShopId, sp.ProgramId });

        builder.Entity<ShopUser>()
            .HasKey(su => new { su.ShopId, su.UserId });

        builder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany((ApplicationRole r) => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);

        builder.Entity<CustomerShop>()
            .HasKey(cs => new { cs.CustomerId, cs.ShopId });
        builder.Entity<CustomerShop>()
            .HasOne(cs => cs.Customer)
            .WithMany(c => c.CustomerShops)
            .HasForeignKey(cs => cs.CustomerId);
        builder.Entity<CustomerShop>()
            .HasOne(cs => cs.Shop)
            .WithMany(s => s.CustomerShops)
            .HasForeignKey(cs => cs.ShopId);

        // Configure Customer relationships and multi-tenant filtering
        builder.Entity<Customer>().HasQueryFilter(c => c.IsActive);

        builder.Entity<Workspace>()
            .HasQueryFilter(w => w.CustomerId == _tenantService.GetCurrentTenantId());

        builder.Entity<Shop>()
            .HasQueryFilter(s => s.CustomerId == _tenantService.GetCurrentTenantId());

        builder.Entity<CCICustomerPortalApi.Models.Program>()
            .HasQueryFilter(p => p.CustomerId == _tenantService.GetCurrentTenantId());

        builder.Entity<ReportCategory>()
            .HasQueryFilter(rc => rc.CustomerId == _tenantService.GetCurrentTenantId());

        // Configure indexes for performance
        builder.Entity<Shop>()
            .HasIndex(s => new { s.Name, s.City, s.State });

        builder.Entity<Customer>()
            .HasIndex(c => c.Subdomain)
            .IsUnique();

        // Configure cascading deletes
        builder.Entity<Customer>()
            .HasMany(c => c.Workspaces)
            .WithOne(w => w.Customer)
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Workspace>()
            .HasMany(w => w.Reports)
            .WithOne(r => r.Workspace)
            .HasForeignKey(r => r.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Shop>()
            .HasMany(s => s.ShopKpis)
            .WithOne(k => k.Shop)
            .HasForeignKey(k => k.ShopId)
            .OnDelete(DeleteBehavior.Cascade); // Only one cascade allowed

        // Remove the ShopPrograms foreign key constraint to Shop
        // builder.Entity<Shop>()
        //     .HasMany(s => s.ShopPrograms)
        //     .WithOne(sp => sp.Shop)
        //     .HasForeignKey(sp => sp.ShopId)
        //     .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Shop>()
            .HasMany(s => s.ShopUsers)
            .WithOne(su => su.Shop)
            .HasForeignKey(su => su.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Shop>()
            .HasMany(s => s.CustomerShops)
            .WithOne(cs => cs.Shop)
            .HasForeignKey(cs => cs.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ShopProgram>()
            .HasOne(sp => sp.Shop)
            .WithMany(s => s.ShopPrograms)
            .HasForeignKey(sp => sp.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ShopProgram>()
            .HasOne(sp => sp.Program)
            .WithMany(p => p.ShopPrograms)
            .HasForeignKey(sp => sp.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add audit trail and timestamp logic
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity is IHasCreatedAt entity)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}