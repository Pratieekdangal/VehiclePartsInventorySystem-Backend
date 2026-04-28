using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.Services;

public interface ISalesService
{
    Task<IEnumerable<SalesInvoiceDto>> GetAllAsync();
    Task<IEnumerable<SalesInvoiceDto>> GetByCustomerAsync(Guid customerId);
    Task<SalesInvoiceDto> GetByIdAsync(Guid id);
    Task<SalesInvoiceDto> CreateAsync(Guid staffId, CreateSalesInvoiceRequest request);
    Task EmailInvoiceAsync(Guid invoiceId);
    Task<SalesInvoiceDto> RecordPaymentAsync(Guid invoiceId, decimal amount);
}

public class SalesService : ISalesService
{
    public const decimal LoyaltyThreshold = 5000m;
    public const decimal LoyaltyDiscountPercent = 0.10m;

    private readonly IAppDbContext _db;
    private readonly IEmailService _email;

    public SalesService(IAppDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetAllAsync()
    {
        var rows = await _db.SalesInvoices
            .Include(s => s.Customer).Include(s => s.Staff).Include(s => s.Vehicle)
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .OrderByDescending(s => s.InvoiceDate).ToListAsync();
        return rows.Select(Map);
    }

    public async Task<IEnumerable<SalesInvoiceDto>> GetByCustomerAsync(Guid customerId)
    {
        var rows = await _db.SalesInvoices
            .Include(s => s.Customer).Include(s => s.Staff).Include(s => s.Vehicle)
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.InvoiceDate).ToListAsync();
        return rows.Select(Map);
    }

    public async Task<SalesInvoiceDto> GetByIdAsync(Guid id)
    {
        var s = await _db.SalesInvoices
            .Include(x => x.Customer).Include(x => x.Staff).Include(x => x.Vehicle)
            .Include(x => x.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException("Invoice not found.");
        return Map(s);
    }

    public async Task<SalesInvoiceDto> CreateAsync(Guid staffId, CreateSalesInvoiceRequest request)
    {
        if (request.Items is null || !request.Items.Any())
            throw new InvalidOperationException("Invoice must have at least one item.");

        var customer = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.CustomerId && u.Role == UserRole.Customer)
            ?? throw new KeyNotFoundException("Customer not found.");

        var invoice = new SalesInvoice
        {
            CustomerId = request.CustomerId,
            StaffId = staffId,
            VehicleId = request.VehicleId,
            DueDate = request.DueDate,
            Notes = request.Notes,
            InvoiceNumber = $"SI-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}"
        };

        decimal subtotal = 0;
        foreach (var it in request.Items)
        {
            var part = await _db.Parts.FindAsync(it.PartId)
                ?? throw new KeyNotFoundException($"Part {it.PartId} not found.");

            if (part.StockQuantity < it.Quantity)
                throw new InvalidOperationException($"Insufficient stock for {part.Name}. Available: {part.StockQuantity}.");

            var line = new SalesInvoiceItem
            {
                PartId = part.Id,
                Quantity = it.Quantity,
                UnitPrice = part.SellingPrice,
                LineTotal = it.Quantity * part.SellingPrice
            };
            invoice.Items.Add(line);
            subtotal += line.LineTotal;

            part.StockQuantity -= it.Quantity;
            part.UpdatedAt = DateTime.UtcNow;
        }

        invoice.Subtotal = subtotal;
        if (subtotal > LoyaltyThreshold)
        {
            invoice.IsLoyaltyDiscountApplied = true;
            invoice.DiscountAmount = Math.Round(subtotal * LoyaltyDiscountPercent, 2);
        }
        invoice.TotalAmount = invoice.Subtotal - invoice.DiscountAmount;
        invoice.AmountPaid = Math.Min(request.AmountPaid, invoice.TotalAmount);
        invoice.BalanceDue = invoice.TotalAmount - invoice.AmountPaid;
        invoice.PaymentStatus = invoice.BalanceDue == 0
            ? PaymentStatus.Paid
            : (invoice.AmountPaid > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Pending);

        _db.SalesInvoices.Add(invoice);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(invoice.Id);
    }

    public async Task<SalesInvoiceDto> RecordPaymentAsync(Guid invoiceId, decimal amount)
    {
        var invoice = await _db.SalesInvoices.FirstOrDefaultAsync(x => x.Id == invoiceId)
            ?? throw new KeyNotFoundException("Invoice not found.");
        invoice.AmountPaid = Math.Min(invoice.AmountPaid + amount, invoice.TotalAmount);
        invoice.BalanceDue = invoice.TotalAmount - invoice.AmountPaid;
        invoice.PaymentStatus = invoice.BalanceDue == 0
            ? PaymentStatus.Paid
            : (invoice.AmountPaid > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Pending);
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(invoiceId);
    }

    public async Task EmailInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _db.SalesInvoices
            .Include(s => s.Customer).Include(s => s.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(s => s.Id == invoiceId)
            ?? throw new KeyNotFoundException("Invoice not found.");

        var lines = string.Join("", invoice.Items.Select(i =>
            $"<tr><td>{i.Part.Name}</td><td>{i.Quantity}</td><td>Rs. {i.UnitPrice:F2}</td><td>Rs. {i.LineTotal:F2}</td></tr>"));

        var html = $@"<h2>Invoice {invoice.InvoiceNumber}</h2>
<p>Dear {invoice.Customer.FullName},</p>
<p>Thank you for your purchase. Please find your invoice details below.</p>
<table border='1' cellpadding='6' cellspacing='0'>
  <tr><th>Part</th><th>Qty</th><th>Unit Price</th><th>Total</th></tr>
  {lines}
</table>
<p><strong>Subtotal:</strong> Rs. {invoice.Subtotal:F2}<br/>
<strong>Discount:</strong> Rs. {invoice.DiscountAmount:F2}{(invoice.IsLoyaltyDiscountApplied ? " (Loyalty 10%)" : "")}<br/>
<strong>Total:</strong> Rs. {invoice.TotalAmount:F2}<br/>
<strong>Paid:</strong> Rs. {invoice.AmountPaid:F2}<br/>
<strong>Balance Due:</strong> Rs. {invoice.BalanceDue:F2}</p>
<p>Regards,<br/>Vehicle Parts & Services</p>";

        await _email.SendAsync(invoice.Customer.Email, $"Invoice {invoice.InvoiceNumber}", html);
    }

    private static SalesInvoiceDto Map(SalesInvoice s) => new(
        s.Id, s.InvoiceNumber, s.CustomerId, s.Customer.FullName, s.Customer.Email,
        s.StaffId, s.Staff.FullName, s.VehicleId, s.Vehicle?.VehicleNumber,
        s.InvoiceDate, s.Subtotal, s.DiscountAmount, s.TotalAmount, s.AmountPaid, s.BalanceDue,
        s.PaymentStatus, s.DueDate, s.IsLoyaltyDiscountApplied, s.Notes,
        s.Items.Select(i => new SalesInvoiceItemDto(
            i.PartId, i.Part.Name, i.Part.PartCode, i.Quantity, i.UnitPrice, i.LineTotal)),
        s.CreatedAt
    );
}
