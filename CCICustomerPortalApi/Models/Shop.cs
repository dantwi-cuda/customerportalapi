namespace CCICustomerPortalApi.Models;

public class Shop
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string Country { get; set; } = null!;
    public bool IsActive { get; set; }
    public int? CustomerId { get; set; } // Remove or make nullable for many-to-many
    public string? BusinessKey { get; set; }
    public int? ParentID { get; set; }

    // Navigation properties
    // Remove or comment out the following line, as Shop now supports many-to-many:
    // public Customer Customer { get; set; } = null!;
    public ICollection<ShopProgram> ShopPrograms { get; set; } = new List<ShopProgram>();
    public ICollection<ShopUser> ShopUsers { get; set; } = new List<ShopUser>();
    public ICollection<ShopKpi> ShopKpis { get; set; } = new List<ShopKpi>();
    public ICollection<CustomerShop> CustomerShops { get; set; } = new List<CustomerShop>();
}