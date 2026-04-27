using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.Services;

public interface IReportService
{
    Task<DashboardStatsDto> GetAdminStatsAsync();
    Task<StaffDashboardStatsDto> GetStaffStatsAsync();
    Task<CustomerDashboardStatsDto> GetCustomerStatsAsync(Guid customerId);
    Task<FinancialReportDto> GetFinancialReportAsync(string range);
    Task<IEnumerable<CustomerSpendingDto>> GetTopSpendersAsync(int top = 10);
    Task<IEnumerable<CustomerSpendingDto>> GetRegularsAsync(int minInvoices = 3);
    Task<IEnumerable<CustomerSpendingDto>> GetPendingCreditsAsync();
}

public class ReportService : IReportService
{
    private readonly IAppDbContext _db;
    public ReportService(IAppDbContext db) => _db = db;

    public async Task<DashboardStatsDto> GetAdminStatsAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalParts = await _db.Parts.CountAsync(p => p.IsActive);
        var lowStock = await _db.Parts.CountAsync(p => p.IsActive && p.StockQuantity < p.LowStockThreshold);
        var vendors = await _db.Vendors.CountAsync(v => v.IsActive);
        var customers = await _db.Users.CountAsync(u => u.Role == UserRole.Customer);
        var staff = await _db.Users.CountAsync(u => u.Role == UserRole.Staff && u.IsActive);

        var monthlyInvoices = await _db.SalesInvoices
            .Where(s => s.InvoiceDate >= monthStart)
            .Select(s => new { s.TotalAmount, Cost = s.Items.Sum(i => i.Quantity * i.Part.PurchasePrice) })
            .ToListAsync();
        var revenue = monthlyInvoices.Sum(x => x.TotalAmount);
        var profit = revenue - monthlyInvoices.Sum(x => x.Cost);

        var pendingAppointments = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
        var pendingPartRequests = await _db.PartRequests.CountAsync(r => r.Status == PartRequestStatus.Pending);
        var overdueInvoices = await _db.SalesInvoices.CountAsync(s =>
            s.BalanceDue > 0 && s.DueDate.HasValue && s.DueDate.Value < now);

        return new DashboardStatsDto(totalParts, lowStock, vendors, customers, staff,
            revenue, profit, pendingAppointments, pendingPartRequests, overdueInvoices);
    }

    public async Task<StaffDashboardStatsDto> GetStaffStatsAsync()
    {
        var todayStart = DateTime.UtcNow.Date;
        var customersToday = await _db.Users.CountAsync(u => u.Role == UserRole.Customer && u.CreatedAt >= todayStart);
        var salesToday = await _db.SalesInvoices.CountAsync(s => s.InvoiceDate >= todayStart);
        var salesAmount = await _db.SalesInvoices.Where(s => s.InvoiceDate >= todayStart).SumAsync(s => (decimal?)s.TotalAmount) ?? 0m;
        var pendingCount = await _db.SalesInvoices.CountAsync(s => s.BalanceDue > 0);
        var pendingAmount = await _db.SalesInvoices.SumAsync(s => (decimal?)s.BalanceDue) ?? 0m;
        return new StaffDashboardStatsDto(customersToday, salesToday, salesAmount, pendingCount, pendingAmount);
    }

    public async Task<CustomerDashboardStatsDto> GetCustomerStatsAsync(Guid customerId)
    {
        var vehicles = await _db.Vehicles.CountAsync(v => v.CustomerId == customerId);
        var upcoming = await _db.Appointments.CountAsync(a => a.CustomerId == customerId
            && a.Status != AppointmentStatus.Cancelled && a.AppointmentDate >= DateTime.UtcNow);
        var pendingReq = await _db.PartRequests.CountAsync(r => r.CustomerId == customerId && r.Status == PartRequestStatus.Pending);
        var invoices = await _db.SalesInvoices.Where(s => s.CustomerId == customerId)
            .Select(s => new { s.TotalAmount, s.BalanceDue }).ToListAsync();
        return new CustomerDashboardStatsDto(vehicles, upcoming, pendingReq, invoices.Count,
            invoices.Sum(x => x.TotalAmount), invoices.Sum(x => x.BalanceDue));
    }

    public async Task<FinancialReportDto> GetFinancialReportAsync(string range)
    {
        var now = DateTime.UtcNow;
        DateTime from;
        Func<DateTime, string> bucket;

        switch (range.ToLower())
        {
            case "daily":
                from = now.AddDays(-30).Date;
                bucket = d => d.ToString("yyyy-MM-dd");
                break;
            case "yearly":
                from = new DateTime(now.Year - 4, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                bucket = d => d.Year.ToString();
                break;
            case "monthly":
            default:
                from = now.AddMonths(-11).Date;
                bucket = d => d.ToString("yyyy-MM");
                range = "monthly";
                break;
        }

        var raw = await _db.SalesInvoices
            .Where(s => s.InvoiceDate >= from)
            .Select(s => new { s.InvoiceDate, s.TotalAmount, Cost = s.Items.Sum(i => i.Quantity * i.Part.PurchasePrice) })
            .ToListAsync();

        var rows = raw.GroupBy(x => bucket(x.InvoiceDate))
            .OrderBy(g => g.Key)
            .Select(g => new FinancialReportRow(
                g.Key, g.Sum(x => x.TotalAmount), g.Sum(x => x.Cost),
                g.Sum(x => x.TotalAmount - x.Cost), g.Count()))
            .ToList();

        return new FinancialReportDto(range,
            rows, rows.Sum(r => r.Revenue), rows.Sum(r => r.Cost), rows.Sum(r => r.Profit));
    }

    public async Task<IEnumerable<CustomerSpendingDto>> GetTopSpendersAsync(int top = 10)
    {
        var aggregates = await _db.SalesInvoices
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                Count = g.Count(),
                Spent = g.Sum(x => x.TotalAmount),
                Pending = g.Sum(x => x.BalanceDue)
            })
            .ToListAsync();

        var ids = aggregates.Select(a => a.CustomerId).ToList();
        var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

        return aggregates
            .OrderByDescending(a => a.Spent)
            .Take(top)
            .Join(users, a => a.CustomerId, u => u.Id, (a, u) => new CustomerSpendingDto(
                u.Id, u.FullName, u.Email, u.PhoneNumber, a.Count, a.Spent, a.Pending))
            .ToList();
    }

    public async Task<IEnumerable<CustomerSpendingDto>> GetRegularsAsync(int minInvoices = 3)
    {
        var aggregates = await _db.SalesInvoices
            .GroupBy(s => s.CustomerId)
            .Where(g => g.Count() >= minInvoices)
            .Select(g => new
            {
                CustomerId = g.Key,
                Count = g.Count(),
                Spent = g.Sum(x => x.TotalAmount),
                Pending = g.Sum(x => x.BalanceDue)
            })
            .ToListAsync();

        var ids = aggregates.Select(a => a.CustomerId).ToList();
        var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

        return aggregates
            .OrderByDescending(a => a.Count)
            .Join(users, a => a.CustomerId, u => u.Id, (a, u) => new CustomerSpendingDto(
                u.Id, u.FullName, u.Email, u.PhoneNumber, a.Count, a.Spent, a.Pending))
            .ToList();
    }

    public async Task<IEnumerable<CustomerSpendingDto>> GetPendingCreditsAsync()
    {
        var aggregates = await _db.SalesInvoices
            .Where(s => s.BalanceDue > 0)
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                Count = g.Count(),
                Spent = g.Sum(x => x.TotalAmount),
                Pending = g.Sum(x => x.BalanceDue)
            })
            .ToListAsync();

        var ids = aggregates.Select(a => a.CustomerId).ToList();
        var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

        return aggregates
            .OrderByDescending(a => a.Pending)
            .Join(users, a => a.CustomerId, u => u.Id, (a, u) => new CustomerSpendingDto(
                u.Id, u.FullName, u.Email, u.PhoneNumber, a.Count, a.Spent, a.Pending))
            .ToList();
    }
}
