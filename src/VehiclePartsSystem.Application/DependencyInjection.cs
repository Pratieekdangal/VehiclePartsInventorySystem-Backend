using Microsoft.Extensions.DependencyInjection;
using VehiclePartsSystem.Application.Services;

namespace VehiclePartsSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IPartService, PartService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IPartRequestService, PartRequestService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}
