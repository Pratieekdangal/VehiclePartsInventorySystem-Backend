using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;

namespace VehiclePartsSystem.Application.Services;

public interface IPurchaseService
{
    Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync();
    Task<PurchaseInvoiceDto> GetByIdAsync(Guid id);
    Task<PurchaseInvoiceDto> CreateAsync(Guid adminId, CreatePurchaseInvoiceRequest request);
}

public class PurchaseService : IPurchaseService
{
    private readonly IAppDbContext _db;
    public PurchaseService(IAppDbContext db) => _db = db;

    public async Task<IEnumerable<PurchaseInvoiceDto>> GetAllAsync()
    {
        var rows = await _db.PurchaseInvoices
            .Include(p => p.Vendor)
            .Include(p => p.Items).ThenInclude(i => i.Part)
            .OrderByDescending(p => p.InvoiceDate)
            .ToListAsync();
        return rows.Select(Map);
    }

    public async Task<PurchaseInvoiceDto> GetByIdAsync(Guid id)
    {
        var p = await _db.PurchaseInvoices
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException("Invoice not found.");
        return Map(p);
    }

    public async Task<PurchaseInvoiceDto> CreateAsync(Guid adminId, CreatePurchaseInvoiceRequest request)
    {
        if (request.Items is null || !request.Items.Any())
            throw new InvalidOperationException("Invoice must have at least one item.");

        var vendor = await _db.Vendors.FindAsync(request.VendorId)
            ?? throw new KeyNotFoundException("Vendor not found.");

        var invoice = new PurchaseInvoice
        {
            VendorId = request.VendorId,
            CreatedByAdminId = adminId,
            InvoiceNumber = $"PI-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}",
            Notes = request.Notes
        };

        decimal total = 0;
        foreach (var it in request.Items)
        {
            var part = await _db.Parts.FindAsync(it.PartId)
                ?? throw new KeyNotFoundException($"Part {it.PartId} not found.");

            var line = new PurchaseInvoiceItem
            {
                PartId = it.PartId,
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice,
                LineTotal = it.Quantity * it.UnitPrice
            };
            invoice.Items.Add(line);
            total += line.LineTotal;

            part.StockQuantity += it.Quantity;
            part.PurchasePrice = it.UnitPrice;
            part.UpdatedAt = DateTime.UtcNow;
        }
        invoice.TotalAmount = total;

        _db.PurchaseInvoices.Add(invoice);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(invoice.Id);
    }

    private static PurchaseInvoiceDto Map(PurchaseInvoice p) => new(
        p.Id, p.InvoiceNumber, p.VendorId, p.Vendor.Name,
        p.InvoiceDate, p.TotalAmount, p.Notes,
        p.Items.Select(i => new PurchaseInvoiceItemDto(
            i.PartId, i.Part.Name, i.Part.PartCode, i.Quantity, i.UnitPrice, i.LineTotal)),
        p.CreatedAt
    );
}
