using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;

namespace VehiclePartsSystem.Application.Services;

public interface IVendorService
{
    Task<IEnumerable<VendorDto>> GetAllAsync();
    Task<VendorDto> GetByIdAsync(Guid id);
    Task<VendorDto> CreateAsync(CreateVendorRequest request);
    Task<VendorDto> UpdateAsync(Guid id, UpdateVendorRequest request);
    Task DeleteAsync(Guid id);
}

public class VendorService : IVendorService
{
    private readonly IAppDbContext _db;

    public VendorService(IAppDbContext db) => _db = db;

    public async Task<IEnumerable<VendorDto>> GetAllAsync()
    {
        return await _db.Vendors
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VendorDto(v.Id, v.Name, v.ContactPerson, v.Email,
                v.PhoneNumber, v.Address, v.IsActive, v.CreatedAt))
            .ToListAsync();
    }

    public async Task<VendorDto> GetByIdAsync(Guid id)
    {
        var v = await _db.Vendors.FindAsync(id) ?? throw new KeyNotFoundException("Vendor not found.");
        return new VendorDto(v.Id, v.Name, v.ContactPerson, v.Email,
            v.PhoneNumber, v.Address, v.IsActive, v.CreatedAt);
    }

    public async Task<VendorDto> CreateAsync(CreateVendorRequest request)
    {
        var v = new Vendor
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address
        };
        _db.Vendors.Add(v);
        await _db.SaveChangesAsync();
        return new VendorDto(v.Id, v.Name, v.ContactPerson, v.Email, v.PhoneNumber, v.Address, v.IsActive, v.CreatedAt);
    }

    public async Task<VendorDto> UpdateAsync(Guid id, UpdateVendorRequest request)
    {
        var v = await _db.Vendors.FindAsync(id) ?? throw new KeyNotFoundException("Vendor not found.");
        v.Name = request.Name;
        v.ContactPerson = request.ContactPerson;
        v.Email = request.Email;
        v.PhoneNumber = request.PhoneNumber;
        v.Address = request.Address;
        v.IsActive = request.IsActive;
        v.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new VendorDto(v.Id, v.Name, v.ContactPerson, v.Email, v.PhoneNumber, v.Address, v.IsActive, v.CreatedAt);
    }

    public async Task DeleteAsync(Guid id)
    {
        var v = await _db.Vendors.FindAsync(id) ?? throw new KeyNotFoundException("Vendor not found.");
        _db.Vendors.Remove(v);
        await _db.SaveChangesAsync();
    }
}
