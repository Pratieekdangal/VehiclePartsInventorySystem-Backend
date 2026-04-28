using VehiclePartsSystem.Domain.Common;

namespace VehiclePartsSystem.Domain.Entities;

public class Part : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string PartCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public string? CompatibleMake { get; set; }
    public string? CompatibleModel { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
}
