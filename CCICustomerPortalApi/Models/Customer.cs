namespace CCICustomerPortalApi.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Subdomain { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<CustomerUser> CustomerUsers { get; set; } = new List<CustomerUser>();
    public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
    public ICollection<Shop> Shops { get; set; } = new List<Shop>();
    public ICollection<Program> Programs { get; set; } = new List<Program>();
    public ICollection<ReportCategory> ReportCategories { get; set; } = new List<ReportCategory>();
    public ICollection<CustomerShop> CustomerShops { get; set; } = new List<CustomerShop>();
}