namespace CCICustomerPortalApi.Models.DTOs;

public class ShopDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Country { get; set; } = null!;
    public bool IsActive { get; set; }
    public List<string> ProgramNames { get; set; } = new();
    public List<ShopKpiDto> KPIs { get; set; } = new();
}