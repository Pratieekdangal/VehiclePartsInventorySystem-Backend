namespace VehiclePartsSystem.Application.DTOs;

public record FinancialReportRow(string Period, decimal Revenue, decimal Cost, decimal Profit, int InvoiceCount);
public record FinancialReportDto(string Range, IEnumerable<FinancialReportRow> Rows, decimal TotalRevenue, decimal TotalCost, decimal TotalProfit);

public record DashboardStatsDto(
    int TotalParts, int LowStockCount, int VendorCount,
    int CustomerCount, int StaffCount,
    decimal MonthlyRevenue, decimal MonthlyProfit,
    int PendingAppointments, int PendingPartRequests,
    int OverdueInvoices
);

public record CustomerSpendingDto(
    Guid CustomerId, string FullName, string Email, string PhoneNumber,
    int InvoiceCount, decimal TotalSpent, decimal PendingCredit
);

public record StaffDashboardStatsDto(
    int CustomersToday, int SalesToday, decimal SalesAmountToday,
    int PendingCreditCount, decimal PendingCreditAmount,
    int LoyaltyAppliedToday, decimal LoyaltySavingsToday
);

public record CustomerDashboardStatsDto(
    int VehicleCount, int UpcomingAppointments, int PendingPartRequests,
    int TotalPurchases, decimal TotalSpent, decimal PendingCredit
);

public record NotificationDto(
    Guid Id, string Title, string Message, string Type,
    bool IsRead, string? RelatedEntityId, DateTime CreatedAt
);
