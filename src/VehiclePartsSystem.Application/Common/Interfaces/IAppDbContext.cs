using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Domain.Entities;

namespace VehiclePartsSystem.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Vendor> Vendors { get; }
    DbSet<Part> Parts { get; }
    DbSet<PurchaseInvoice> PurchaseInvoices { get; }
    DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; }
    DbSet<SalesInvoice> SalesInvoices { get; }
    DbSet<SalesInvoiceItem> SalesInvoiceItems { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Review> Reviews { get; }
    DbSet<PartRequest> PartRequests { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
