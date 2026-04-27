using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerSummaryDto>> SearchAsync(string? query, string? vehicleNumber);
    Task<CustomerDetailDto> GetByIdAsync(Guid id);
    Task<CustomerSummaryDto> CreateByStaffAsync(CreateCustomerByStaffRequest request);
    Task<CustomerDetailDto> UpdateProfileAsync(Guid customerId, UpdateProfileRequest request);

    Task<IEnumerable<VehicleDto>> GetVehiclesAsync(Guid customerId);
    Task<VehicleDto> AddVehicleAsync(Guid customerId, CreateVehicleRequest request);
    Task<VehicleDto> UpdateVehicleAsync(Guid customerId, Guid vehicleId, UpdateVehicleRequest request);
    Task DeleteVehicleAsync(Guid customerId, Guid vehicleId);
}

public class CustomerService : ICustomerService
{
    private readonly IAppDbContext _db;
    public CustomerService(IAppDbContext db) => _db = db;

    public async Task<IEnumerable<CustomerSummaryDto>> SearchAsync(string? query, string? vehicleNumber)
    {
        var q = _db.Users.Where(u => u.Role == UserRole.Customer).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var s = query.ToLower();
            q = q.Where(u => u.FullName.ToLower().Contains(s)
                          || u.Email.ToLower().Contains(s)
                          || u.PhoneNumber.Contains(s)
                          || u.Id.ToString().ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(vehicleNumber))
        {
            var vn = vehicleNumber.ToLower();
            q = q.Where(u => u.Vehicles.Any(v => v.VehicleNumber.ToLower().Contains(vn)));
        }

        var users = await q.OrderByDescending(u => u.CreatedAt).ToListAsync();
        var ids = users.Select(u => u.Id).ToList();

        var totals = await _db.SalesInvoices
            .Where(i => ids.Contains(i.CustomerId))
            .GroupBy(i => i.CustomerId)
            .Select(g => new { CustomerId = g.Key, Spent = g.Sum(x => x.TotalAmount), Pending = g.Sum(x => x.BalanceDue) })
            .ToListAsync();

        var vehicleCounts = await _db.Vehicles
            .Where(v => ids.Contains(v.CustomerId))
            .GroupBy(v => v.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToListAsync();

        return users.Select(u =>
        {
            var t = totals.FirstOrDefault(x => x.CustomerId == u.Id);
            var v = vehicleCounts.FirstOrDefault(x => x.CustomerId == u.Id);
            return new CustomerSummaryDto(u.Id, u.FullName, u.Email, u.PhoneNumber, u.Address, u.IsActive,
                v?.Count ?? 0, t?.Spent ?? 0m, t?.Pending ?? 0m, u.CreatedAt);
        });
    }

    public async Task<CustomerDetailDto> GetByIdAsync(Guid id)
    {
        var u = await _db.Users.Include(x => x.Vehicles)
                .FirstOrDefaultAsync(x => x.Id == id && x.Role == UserRole.Customer)
                ?? throw new KeyNotFoundException("Customer not found.");

        var invoices = await _db.SalesInvoices
            .Where(i => i.CustomerId == id)
            .Select(i => new { i.TotalAmount, i.BalanceDue })
            .ToListAsync();

        var vehicles = u.Vehicles.Select(v =>
            new VehicleDto(v.Id, v.CustomerId, v.VehicleNumber, v.Make, v.Model, v.Year,
                v.Color, v.Mileage, v.LastServiceDate, v.CreatedAt));

        return new CustomerDetailDto(
            u.Id, u.FullName, u.Email, u.PhoneNumber, u.Address, u.IsActive,
            vehicles, invoices.Sum(x => x.TotalAmount), invoices.Sum(x => x.BalanceDue),
            invoices.Count, u.CreatedAt);
    }

    public async Task<CustomerSummaryDto> CreateByStaffAsync(CreateCustomerByStaffRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already registered.");

        var u = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Role = UserRole.Customer,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        if (request.Vehicles is not null)
        {
            foreach (var v in request.Vehicles)
                u.Vehicles.Add(new Vehicle
                {
                    VehicleNumber = v.VehicleNumber,
                    Make = v.Make,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    Mileage = v.Mileage
                });
        }
        _db.Users.Add(u);
        await _db.SaveChangesAsync();

        return new CustomerSummaryDto(u.Id, u.FullName, u.Email, u.PhoneNumber, u.Address, u.IsActive,
            u.Vehicles.Count, 0m, 0m, u.CreatedAt);
    }

    public async Task<CustomerDetailDto> UpdateProfileAsync(Guid customerId, UpdateProfileRequest request)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == customerId)
                ?? throw new KeyNotFoundException("User not found.");
        u.FullName = request.FullName;
        u.PhoneNumber = request.PhoneNumber;
        u.Address = request.Address;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(customerId);
    }

    public async Task<IEnumerable<VehicleDto>> GetVehiclesAsync(Guid customerId)
    {
        return await _db.Vehicles.Where(v => v.CustomerId == customerId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VehicleDto(v.Id, v.CustomerId, v.VehicleNumber, v.Make, v.Model,
                v.Year, v.Color, v.Mileage, v.LastServiceDate, v.CreatedAt))
            .ToListAsync();
    }

    public async Task<VehicleDto> AddVehicleAsync(Guid customerId, CreateVehicleRequest request)
    {
        if (await _db.Vehicles.AnyAsync(v => v.VehicleNumber == request.VehicleNumber))
            throw new InvalidOperationException("Vehicle number already registered.");

        var v = new Vehicle
        {
            CustomerId = customerId,
            VehicleNumber = request.VehicleNumber,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Color = request.Color,
            Mileage = request.Mileage
        };
        _db.Vehicles.Add(v);
        await _db.SaveChangesAsync();
        return new VehicleDto(v.Id, v.CustomerId, v.VehicleNumber, v.Make, v.Model, v.Year,
            v.Color, v.Mileage, v.LastServiceDate, v.CreatedAt);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(Guid customerId, Guid vehicleId, UpdateVehicleRequest request)
    {
        var v = await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.CustomerId == customerId)
                ?? throw new KeyNotFoundException("Vehicle not found.");
        v.VehicleNumber = request.VehicleNumber;
        v.Make = request.Make;
        v.Model = request.Model;
        v.Year = request.Year;
        v.Color = request.Color;
        v.Mileage = request.Mileage;
        v.LastServiceDate = request.LastServiceDate;
        v.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new VehicleDto(v.Id, v.CustomerId, v.VehicleNumber, v.Make, v.Model, v.Year,
            v.Color, v.Mileage, v.LastServiceDate, v.CreatedAt);
    }

    public async Task DeleteVehicleAsync(Guid customerId, Guid vehicleId)
    {
        var v = await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.CustomerId == customerId)
                ?? throw new KeyNotFoundException("Vehicle not found.");
        _db.Vehicles.Remove(v);
        await _db.SaveChangesAsync();
    }
}
