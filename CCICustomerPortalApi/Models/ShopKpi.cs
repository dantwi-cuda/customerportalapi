namespace CCICustomerPortalApi.Models;

public class ShopKpi
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public int ShopPropertiesId { get; set; }
    public int dimBusinessUnitId { get; set; }
    public int KPIYear { get; set; }
    public int KPIMonth { get; set; }
    public decimal? KPIValue { get; set; }
    public decimal? KPIGoal { get; set; }
    public decimal? KPIThreshold { get; set; }
    public string PropertyName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string UnitType { get; set; } = null!;
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = null!;
    public int AttributeSortOrder { get; set; }
    public int AttributeCategoryId { get; set; }
    public string AttributeCategoryDescription { get; set; } = null!;
    public int AttributeCategorySortOrder { get; set; }
    public int AttributeUnitId { get; set; }
    public bool? IsTable { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
    public string RowModifiedBy { get; set; } = null!;
    public DateTime? RowModifiedOn { get; set; }
    public decimal? KPIBMSValue { get; set; }
    public DateTime Timestamp { get; set; }

    // Navigation properties
    public Shop Shop { get; set; } = null!;
}