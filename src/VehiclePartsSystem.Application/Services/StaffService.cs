using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.Services;

public interface IStaffService
{
    Task<IEnumerable<StaffDto>> GetAllAsync();
    Task<StaffDto> CreateAsync(CreateStaffRequest request);
    Task<StaffDto> UpdateAsync(Guid id, UpdateStaffRequest request);
    Task DeleteAsync(Guid id);
    Task ResetPasswordAsync(Guid id, string newPassword);
}

public class StaffService : IStaffService
{
    private readonly IAppDbContext _db;
    public StaffService(IAppDbContext db) => _db = db;

    public async Task<IEnumerable<StaffDto>> GetAllAsync()
    {
        return await _db.Users
            .Where(u => u.Role == UserRole.Staff)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new StaffDto(u.Id, u.FullName, u.Email, u.PhoneNumber, u.Role, u.IsActive, u.CreatedAt))
            .ToListAsync();
    }

    public async Task<StaffDto> CreateAsync(CreateStaffRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email already in use.");

        var u = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Role = UserRole.Staff,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        return new StaffDto(u.Id, u.FullName, u.Email, u.PhoneNumber, u.Role, u.IsActive, u.CreatedAt);
    }

    public async Task<StaffDto> UpdateAsync(Guid id, UpdateStaffRequest request)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id && x.Role == UserRole.Staff)
            ?? throw new KeyNotFoundException("Staff not found.");
        u.FullName = request.FullName;
        u.PhoneNumber = request.PhoneNumber;
        u.Address = request.Address;
        u.IsActive = request.IsActive;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new StaffDto(u.Id, u.FullName, u.Email, u.PhoneNumber, u.Role, u.IsActive, u.CreatedAt);
    }

    public async Task DeleteAsync(Guid id)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id && x.Role == UserRole.Staff)
            ?? throw new KeyNotFoundException("Staff not found.");
        u.IsActive = false;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(Guid id, string newPassword)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Id == id && x.Role == UserRole.Staff)
            ?? throw new KeyNotFoundException("Staff not found.");
        u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
