using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.DTOs;

public record SalesInvoiceItemDto(
    Guid PartId, string PartName, string PartCode,
    int Quantity, decimal UnitPrice, decimal LineTotal
);

public record SalesInvoiceDto(
    Guid Id, string InvoiceNumber,
    Guid CustomerId, string CustomerName, string CustomerEmail,
    Guid StaffId, string StaffName,
    Guid? VehicleId, string? VehicleNumber,
    DateTime InvoiceDate, decimal Subtotal, decimal DiscountAmount,
    decimal TotalAmount, decimal AmountPaid, decimal BalanceDue,
    PaymentStatus PaymentStatus, DateTime? DueDate, bool IsLoyaltyDiscountApplied,
    string? Notes, IEnumerable<SalesInvoiceItemDto> Items, DateTime CreatedAt
);

public record CreateSalesInvoiceItem(Guid PartId, int Quantity);

public record CreateSalesInvoiceRequest(
    Guid CustomerId, Guid? VehicleId, decimal AmountPaid,
    DateTime? DueDate, string? Notes, IEnumerable<CreateSalesInvoiceItem> Items
);
