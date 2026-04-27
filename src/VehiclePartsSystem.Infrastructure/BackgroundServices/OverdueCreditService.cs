using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.Services;
using VehiclePartsSystem.Infrastructure.Data;

namespace VehiclePartsSystem.Infrastructure.BackgroundServices;

public class OverdueCreditService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<OverdueCreditService> _logger;

    public OverdueCreditService(IServiceProvider provider, ILogger<OverdueCreditService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ScanAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Overdue credit scan failed"); }

            await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
        }
    }

    private async Task ScanAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var overdue = await db.SalesInvoices
            .Include(s => s.Customer)
            .Where(s => s.BalanceDue > 0 && s.InvoiceDate < cutoff)
            .ToListAsync(ct);

        foreach (var invoice in overdue)
        {
            var marker = $"overdue:{invoice.Id}:{DateTime.UtcNow:yyyyMM}";
            var alreadySent = await db.Notifications.AnyAsync(n => n.RelatedEntityId == marker, ct);
            if (alreadySent) continue;

            try
            {
                await email.SendAsync(invoice.Customer.Email,
                    $"Payment overdue — Invoice {invoice.InvoiceNumber}",
                    $@"<p>Dear {invoice.Customer.FullName},</p>
<p>Our records show invoice <strong>{invoice.InvoiceNumber}</strong> has an outstanding balance of
<strong>Rs. {invoice.BalanceDue:F2}</strong> overdue by more than 30 days.</p>
<p>Please settle the payment at your earliest convenience.</p>
<p>Regards,<br/>Vehicle Parts & Services</p>");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to email overdue notice for {InvoiceId}", invoice.Id);
            }

            await notifier.PushAsync(invoice.CustomerId,
                "Payment reminder",
                $"Invoice {invoice.InvoiceNumber} balance Rs. {invoice.BalanceDue:F2} is overdue.",
                "overdue_credit", marker);
        }
    }
}
