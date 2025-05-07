namespace CCICustomerPortalApi.Models;

public class Report
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PowerBiReportId { get; set; } = null!;
    public int WorkspaceId { get; set; }
    public int ReportCategoryId { get; set; }

    // Navigation properties
    public Workspace Workspace { get; set; } = null!;
    public ReportCategory Category { get; set; } = null!;
}