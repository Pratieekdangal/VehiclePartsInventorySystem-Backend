using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VehiclePartsSystem.Application.Services;
using VehiclePartsSystem.Domain.Enums;
using VehiclePartsSystem.Infrastructure.Data;

namespace VehiclePartsSystem.Infrastructure.BackgroundServices;

public class StockMonitorService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<StockMonitorService> _logger;

    public StockMonitorService(IServiceProvider provider, ILogger<StockMonitorService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stock monitor failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task ScanAsync(CancellationToken ct)
    {
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifier = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var lowStock = await db.Parts
            .Where(p => p.IsActive && p.StockQuantity < p.LowStockThreshold)
            .ToListAsync(ct);

        if (lowStock.Count == 0) return;

        var admins = await db.Users.Where(u => u.Role == UserRole.Admin && u.IsActive).Select(u => u.Id).ToListAsync(ct);

        foreach (var admin in admins)
        {
            foreach (var p in lowStock)
            {
                var marker = $"low-stock:{p.Id}:{DateTime.UtcNow:yyyyMMdd}";
                var exists = await db.Notifications.AnyAsync(n => n.UserId == admin && n.RelatedEntityId == marker, ct);
                if (exists) continue;

                await notifier.PushAsync(admin,
                    "Low stock alert",
                    $"{p.Name} ({p.PartCode}) is at {p.StockQuantity} (threshold {p.LowStockThreshold}).",
                    "low_stock", marker);
            }
        }
    }
}
