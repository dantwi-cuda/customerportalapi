using Microsoft.AspNetCore.Identity;

namespace CCICustomerPortalApi.Models;

public class ApplicationUser : IdentityUser, IHasCreatedAt
{
    public bool IsCustomerUser { get; set; }
    public bool IsCCIUser { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ICollection<CustomerUser> CustomerUsers { get; set; } = new List<CustomerUser>();
    public ICollection<UserWorkspace> UserWorkspaces { get; set; } = new List<UserWorkspace>();
}