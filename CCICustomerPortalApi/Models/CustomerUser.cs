namespace CCICustomerPortalApi.Models;

public class CustomerUser : IHasCreatedAt
{
    public int CustomerId { get; set; }
    public string UserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}