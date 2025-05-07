namespace CCICustomerPortalApi.Models;

public class Program
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; }
    public int CustomerId { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<ShopProgram> ShopPrograms { get; set; } = new List<ShopProgram>();
}