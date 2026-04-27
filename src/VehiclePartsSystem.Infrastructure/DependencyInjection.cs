using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Infrastructure.BackgroundServices;
using VehiclePartsSystem.Infrastructure.Data;
using VehiclePartsSystem.Infrastructure.Services;

namespace VehiclePartsSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlite(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddHostedService<StockMonitorService>();
        services.AddHostedService<OverdueCreditService>();

        return services;
    }
}
