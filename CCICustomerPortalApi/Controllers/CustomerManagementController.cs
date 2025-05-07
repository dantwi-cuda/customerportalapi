using CCICustomerPortalApi.Data;
using CCICustomerPortalApi.Models;
using CCICustomerPortalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CCICustomerPortalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireCCIUser")]
public class CustomerManagementController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public CustomerManagementController(
        ApplicationDbContext dbContext,
        ITenantService tenantService,
        IUserService userService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        // Ensure this endpoint is only accessible in admin portal mode
        if (!_tenantService.IsAdminPortalMode())
        {
            return Forbid();
        }

        return await _dbContext.Customers
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        // Ensure this endpoint is only accessible in admin portal mode
        if (!_tenantService.IsAdminPortalMode())
        {
            return Forbid();
        }

        var customer = await _dbContext.Customers.FindAsync(id);

        if (customer == null)
        {
            return NotFound();
        }

        return customer;
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        // Ensure this endpoint is only accessible in admin portal mode
        if (!_tenantService.IsAdminPortalMode())
        {
            return Forbid();
        }

        // Validate subdomain - should be unique
        bool subdomainExists = await _dbContext.Customers
            .AnyAsync(c => c.Subdomain == customer.Subdomain);

        if (subdomainExists)
        {
            return BadRequest(new { error = "Subdomain already in use" });
        }

        // Set creation metadata
        customer.CreatedAt = DateTime.UtcNow;
        customer.IsActive = true;

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
    {
        // Ensure this endpoint is only accessible in admin portal mode
        if (!_tenantService.IsAdminPortalMode())
        {
            return Forbid();
        }

        if (id != customer.Id)
        {
            return BadRequest();
        }

        // Check if subdomain is being changed and if new value is unique
        var existingCustomer = await _dbContext.Customers.FindAsync(id);
        if (existingCustomer == null)
        {
            return NotFound();
        }

        if (existingCustomer.Subdomain != customer.Subdomain)
        {
            bool subdomainExists = await _dbContext.Customers
                .AnyAsync(c => c.Id != id && c.Subdomain == customer.Subdomain);

            if (subdomainExists)
            {
                return BadRequest(new { error = "Subdomain already in use" });
            }
        }

        // Update fields but preserve creation metadata
        existingCustomer.Name = customer.Name;
        existingCustomer.Subdomain = customer.Subdomain;
        existingCustomer.IsActive = customer.IsActive;
        existingCustomer.UpdatedAt = DateTime.UtcNow;
        // Additional fields as needed

        _dbContext.Entry(existingCustomer).State = EntityState.Modified;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CustomerExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateCustomerStatus(int id, [FromBody] bool isActive)
    {
        // Ensure this endpoint is only accessible in admin portal mode
        if (!_tenantService.IsAdminPortalMode())
        {
            return Forbid();
        }

        var customer = await _dbContext.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }

        customer.IsActive = isActive;
        customer.UpdatedAt = DateTime.UtcNow;

        _dbContext.Entry(customer).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{customerId}/users")]
    public async Task<ActionResult<IEnumerable<object>>> GetCustomerUsers(int customerId)
    {
        // Ensure this endpoint is only accessible in admin portal mode
        if (!_tenantService.IsAdminPortalMode())
        {
            return Forbid();
        }

        var customer = await _dbContext.Customers.FindAsync(customerId);
        if (customer == null)
        {
            return NotFound();
        }

        var customerUsers = await _dbContext.CustomerUsers
            .Where(cu => cu.CustomerId == customerId)
            .Select(cu => cu.UserId)
            .ToListAsync();

        var users = await _dbContext.Users
            .Where(u => customerUsers.Contains(u.Id))
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.IsActive,
                u.IsCustomerUser,
                u.IsCCIUser,
                u.CreatedAt,
                u.LastLoginAt,
                Roles = _dbContext.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(_dbContext.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.Name)
                    .ToList()
            })
            .ToListAsync();

        return users;
    }

    private async Task<bool> CustomerExists(int id)
    {
        return await _dbContext.Customers.AnyAsync(e => e.Id == id);
    }
}