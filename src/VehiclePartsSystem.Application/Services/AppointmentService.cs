using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.Services;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();
    Task<IEnumerable<AppointmentDto>> GetByCustomerAsync(Guid customerId);
    Task<AppointmentDto> CreateAsync(Guid customerId, CreateAppointmentRequest request);
    Task<AppointmentDto> UpdateStatusAsync(Guid id, AppointmentStatus status);
    Task DeleteAsync(Guid id);
}

public class AppointmentService : IAppointmentService
{
    private readonly IAppDbContext _db;
    public AppointmentService(IAppDbContext db) => _db = db;

    private static AppointmentDto Map(Appointment a) => new(
        a.Id, a.CustomerId, a.Customer.FullName,
        a.VehicleId, a.Vehicle?.VehicleNumber,
        a.AppointmentDate, a.ServiceType, a.Description, a.Status, a.CreatedAt
    );

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        var rows = await _db.Appointments.Include(a => a.Customer).Include(a => a.Vehicle)
            .OrderByDescending(a => a.AppointmentDate).ToListAsync();
        return rows.Select(Map);
    }

    public async Task<IEnumerable<AppointmentDto>> GetByCustomerAsync(Guid customerId)
    {
        var rows = await _db.Appointments.Include(a => a.Customer).Include(a => a.Vehicle)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.AppointmentDate).ToListAsync();
        return rows.Select(Map);
    }

    public async Task<AppointmentDto> CreateAsync(Guid customerId, CreateAppointmentRequest request)
    {
        if (request.AppointmentDate < DateTime.UtcNow)
            throw new InvalidOperationException("Appointment date must be in the future.");

        var a = new Appointment
        {
            CustomerId = customerId,
            VehicleId = request.VehicleId,
            AppointmentDate = request.AppointmentDate,
            ServiceType = request.ServiceType,
            Description = request.Description
        };
        _db.Appointments.Add(a);
        await _db.SaveChangesAsync();

        var saved = await _db.Appointments.Include(x => x.Customer).Include(x => x.Vehicle)
            .FirstAsync(x => x.Id == a.Id);
        return Map(saved);
    }

    public async Task<AppointmentDto> UpdateStatusAsync(Guid id, AppointmentStatus status)
    {
        var a = await _db.Appointments.Include(x => x.Customer).Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException("Appointment not found.");
        a.Status = status;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(a);
    }

    public async Task DeleteAsync(Guid id)
    {
        var a = await _db.Appointments.FindAsync(id) ?? throw new KeyNotFoundException("Appointment not found.");
        _db.Appointments.Remove(a);
        await _db.SaveChangesAsync();
    }
}
