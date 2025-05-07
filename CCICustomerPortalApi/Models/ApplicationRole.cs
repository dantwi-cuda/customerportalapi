using Microsoft.AspNetCore.Identity;

namespace CCICustomerPortalApi.Models
{
    public class ApplicationRole : IdentityRole
    {
        // You can add custom properties here if needed
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
