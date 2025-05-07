namespace CCICustomerPortalApi.Models;

public class Workspace
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string SystemName { get; set; } = null!;
    public bool IsActive { get; set; }
    public int CustomerId { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<UserWorkspace> UserWorkspaces { get; set; } = new List<UserWorkspace>();
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}