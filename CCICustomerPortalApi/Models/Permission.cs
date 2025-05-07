using System.Collections.Generic;
using CCICustomerPortalApi.Models.DTOs;
using CCICustomerPortalApi.Models;

namespace CCICustomerPortalApi.Models
{
    public class Permission
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public ApplicationRole Role { get; set; } = null!;
        public string PermissionId { get; set; } = string.Empty;
        public Permission Permission { get; set; } = null!;
    }
}