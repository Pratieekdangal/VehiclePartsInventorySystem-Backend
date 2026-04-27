using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Domain.Entities;

namespace VehiclePartsSystem.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        => base.SaveChangesAsync(ct);

    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.PhoneNumber);
            e.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150).IsRequired();
            e.Property(u => u.PhoneNumber).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasIndex(v => v.VehicleNumber).IsUnique();
            e.Property(v => v.VehicleNumber).HasMaxLength(30).IsRequired();
            e.HasOne(v => v.Customer).WithMany(u => u.Vehicles).HasForeignKey(v => v.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Part>(e =>
        {
            e.HasIndex(p => p.PartCode).IsUnique();
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.PartCode).HasMaxLength(50).IsRequired();
            e.Property(p => p.PurchasePrice).HasPrecision(12, 2);
            e.Property(p => p.SellingPrice).HasPrecision(12, 2);
            e.HasOne(p => p.Vendor).WithMany(v => v.Parts).HasForeignKey(p => p.VendorId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PurchaseInvoice>(e =>
        {
            e.HasIndex(p => p.InvoiceNumber).IsUnique();
            e.Property(p => p.TotalAmount).HasPrecision(14, 2);
            e.HasOne(p => p.Vendor).WithMany(v => v.PurchaseInvoices).HasForeignKey(p => p.VendorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.CreatedByAdmin).WithMany().HasForeignKey(p => p.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseInvoiceItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(12, 2);
            e.Property(i => i.LineTotal).HasPrecision(14, 2);
            e.HasOne(i => i.PurchaseInvoice).WithMany(p => p.Items).HasForeignKey(i => i.PurchaseInvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Part).WithMany().HasForeignKey(i => i.PartId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalesInvoice>(e =>
        {
            e.HasIndex(s => s.InvoiceNumber).IsUnique();
            e.Property(s => s.Subtotal).HasPrecision(14, 2);
            e.Property(s => s.DiscountAmount).HasPrecision(12, 2);
            e.Property(s => s.TotalAmount).HasPrecision(14, 2);
            e.Property(s => s.AmountPaid).HasPrecision(14, 2);
            e.Property(s => s.BalanceDue).HasPrecision(14, 2);
            e.HasOne(s => s.Customer).WithMany(u => u.Purchases).HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Staff).WithMany().HasForeignKey(s => s.StaffId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Vehicle).WithMany().HasForeignKey(s => s.VehicleId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SalesInvoiceItem>(e =>
        {
            e.Property(i => i.UnitPrice).HasPrecision(12, 2);
            e.Property(i => i.LineTotal).HasPrecision(14, 2);
            e.HasOne(i => i.SalesInvoice).WithMany(s => s.Items).HasForeignKey(i => i.SalesInvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Part).WithMany().HasForeignKey(i => i.PartId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasOne(a => a.Customer).WithMany(u => u.Appointments).HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Vehicle).WithMany().HasForeignKey(a => a.VehicleId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasOne(r => r.Customer).WithMany(u => u.Reviews).HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PartRequest>(e =>
        {
            e.HasOne(r => r.Customer).WithMany(u => u.PartRequests).HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
