namespace CCICustomerPortalApi.Models;

public class ReportCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CustomerId { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}