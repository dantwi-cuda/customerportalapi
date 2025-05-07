using CCICustomerPortalApi.Models;

public class CustomerShop
{
    public int CustomerId { get; set; }
    public int CustomerShopID { get; set; }
    public Customer Customer { get; set; } = null!;
    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;
}