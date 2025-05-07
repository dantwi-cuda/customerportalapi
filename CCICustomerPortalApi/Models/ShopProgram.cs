namespace CCICustomerPortalApi.Models;

public class ShopProgram
{
    public int ShopId { get; set; }
    public int ProgramId { get; set; }
    public DateTime AssignedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public Shop Shop { get; set; } = null!;
    public Program Program { get; set; } = null!;
}