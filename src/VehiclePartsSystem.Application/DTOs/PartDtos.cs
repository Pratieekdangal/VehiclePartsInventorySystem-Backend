namespace VehiclePartsSystem.Application.DTOs;

public record PartDto(
    Guid Id, string Name, string PartCode, string Category,
    string? Description, decimal PurchasePrice, decimal SellingPrice,
    int StockQuantity, int LowStockThreshold,
    string? CompatibleMake, string? CompatibleModel, string? ImageUrl,
    bool IsActive, Guid? VendorId, string? VendorName, DateTime CreatedAt
);

public record CreatePartRequest(
    string Name, string PartCode, string Category, string? Description,
    decimal PurchasePrice, decimal SellingPrice, int StockQuantity,
    int LowStockThreshold, string? CompatibleMake, string? CompatibleModel,
    string? ImageUrl, Guid? VendorId
);

public record UpdatePartRequest(
    string Name, string Category, string? Description,
    decimal PurchasePrice, decimal SellingPrice, int StockQuantity,
    int LowStockThreshold, string? CompatibleMake, string? CompatibleModel,
    string? ImageUrl, Guid? VendorId, bool IsActive
);
