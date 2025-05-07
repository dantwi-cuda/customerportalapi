namespace CCICustomerPortalApi.Models.DTOs;

public class ReportDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PowerBiReportId { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public int WorkspaceId { get; set; }
    public int ReportCategoryId { get; set; }
}