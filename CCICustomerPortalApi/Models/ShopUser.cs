namespace CCICustomerPortalApi.Models;

public class ShopUser
{
    public int ShopId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime AssignedAt { get; set; }

    // Navigation properties
    public Shop Shop { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}