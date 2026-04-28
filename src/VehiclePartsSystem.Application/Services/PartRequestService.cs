using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.Services;

public interface IPartRequestService
{
    Task<IEnumerable<PartRequestDto>> GetAllAsync();
    Task<IEnumerable<PartRequestDto>> GetByCustomerAsync(Guid customerId);
    Task<PartRequestDto> CreateAsync(Guid customerId, CreatePartRequestRequest request);
    Task<PartRequestDto> RespondAsync(Guid id, RespondToPartRequestRequest request);
}

public class PartRequestService : IPartRequestService
{
    private readonly IAppDbContext _db;
    public PartRequestService(IAppDbContext db) => _db = db;

    private static PartRequestDto Map(PartRequest r) => new(
        r.Id, r.CustomerId, r.Customer.FullName,
        r.PartName, r.Description, r.CompatibleVehicle,
        r.Status, r.AdminResponse, r.CreatedAt);

    public async Task<IEnumerable<PartRequestDto>> GetAllAsync()
    {
        var rows = await _db.PartRequests.Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return rows.Select(Map);
    }

    public async Task<IEnumerable<PartRequestDto>> GetByCustomerAsync(Guid customerId)
    {
        var rows = await _db.PartRequests.Include(r => r.Customer)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return rows.Select(Map);
    }

    public async Task<PartRequestDto> CreateAsync(Guid customerId, CreatePartRequestRequest request)
    {
        var r = new PartRequest
        {
            CustomerId = customerId,
            PartName = request.PartName,
            Description = request.Description,
            CompatibleVehicle = request.CompatibleVehicle,
            Status = PartRequestStatus.Pending
        };
        _db.PartRequests.Add(r);
        await _db.SaveChangesAsync();
        var saved = await _db.PartRequests.Include(x => x.Customer).FirstAsync(x => x.Id == r.Id);
        return Map(saved);
    }

    public async Task<PartRequestDto> RespondAsync(Guid id, RespondToPartRequestRequest request)
    {
        var r = await _db.PartRequests.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Request not found.");
        r.Status = request.Status;
        r.AdminResponse = request.AdminResponse;
        r.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Map(r);
    }
}
