namespace VehiclePartsSystem.Application.DTOs;

public record PurchaseInvoiceItemDto(
    Guid PartId, string PartName, string PartCode,
    int Quantity, decimal UnitPrice, decimal LineTotal
);

public record PurchaseInvoiceDto(
    Guid Id, string InvoiceNumber, Guid VendorId, string VendorName,
    DateTime InvoiceDate, decimal TotalAmount, string? Notes,
    IEnumerable<PurchaseInvoiceItemDto> Items, DateTime CreatedAt
);

public record CreatePurchaseInvoiceItem(Guid PartId, int Quantity, decimal UnitPrice);

public record CreatePurchaseInvoiceRequest(
    Guid VendorId, string? Notes, IEnumerable<CreatePurchaseInvoiceItem> Items
);
