namespace CCICustomerPortalApi.Models.DTOs
{
    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class AssignRoleDto
    {
        public string UserId { get; set; } = string.Empty;
        public string WorkspaceId { get; set; } = string.Empty;
    }

    public class UpdateRolePermissionsDto
    {
        public List<string> Permissions { get; set; } = new();
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public List<string> UserIds { get; set; } = new();
        public List<string> WorkspaceIds { get; set; } = new();
    }
}