namespace CCICustomerPortalApi.Models;

public class UserWorkspace : IHasCreatedAt
{
    public string UserId { get; set; } = null!;
    public int WorkspaceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime AssignedAt { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
}