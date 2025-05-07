namespace CCICustomerPortalApi.Models.DTOs;

public class WorkspaceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string SystemName { get; set; } = null!;
    public bool IsActive { get; set; }
    public List<ReportDto> Reports { get; set; } = new();
}