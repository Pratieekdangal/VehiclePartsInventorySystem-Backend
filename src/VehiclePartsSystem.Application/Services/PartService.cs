using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;

namespace VehiclePartsSystem.Application.Services;

public interface IPartService
{
    Task<IEnumerable<PartDto>> GetAllAsync(string? search = null, string? category = null, bool? lowStockOnly = null);
    Task<PartDto> GetByIdAsync(Guid id);
    Task<PartDto> CreateAsync(CreatePartRequest request);
    Task<PartDto> UpdateAsync(Guid id, UpdatePartRequest request);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<PartDto>> GetLowStockAsync();
}

public class PartService : IPartService
{
    private readonly IAppDbContext _db;
    public PartService(IAppDbContext db) => _db = db;

    private static PartDto Map(Part p) => new(
        p.Id, p.Name, p.PartCode, p.Category, p.Description,
        p.PurchasePrice, p.SellingPrice, p.StockQuantity, p.LowStockThreshold,
        p.CompatibleMake, p.CompatibleModel, p.ImageUrl,
        p.IsActive, p.VendorId, p.Vendor != null ? p.Vendor.Name : null, p.CreatedAt
    );

    public async Task<IEnumerable<PartDto>> GetAllAsync(string? search = null, string? category = null, bool? lowStockOnly = null)
    {
        var q = _db.Parts.Include(p => p.Vendor).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(s) || p.PartCode.ToLower().Contains(s));
        }
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(p => p.Category == category);
        if (lowStockOnly == true)
            q = q.Where(p => p.StockQuantity < p.LowStockThreshold);

        return await q.OrderByDescending(p => p.CreatedAt).Select(p => Map(p)).ToListAsync();
    }

    public async Task<PartDto> GetByIdAsync(Guid id)
    {
        var p = await _db.Parts.Include(x => x.Vendor).FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Part not found.");
        return Map(p);
    }

    public async Task<PartDto> CreateAsync(CreatePartRequest request)
    {
        if (await _db.Parts.AnyAsync(p => p.PartCode == request.PartCode))
            throw new InvalidOperationException("Part code already exists.");

        var p = new Part
        {
            Name = request.Name,
            PartCode = request.PartCode,
            Category = request.Category,
            Description = request.Description,
            PurchasePrice = request.PurchasePrice,
            SellingPrice = request.SellingPrice,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            CompatibleMake = request.CompatibleMake,
            CompatibleModel = request.CompatibleModel,
            ImageUrl = request.ImageUrl,
            VendorId = request.VendorId
        };
        _db.Parts.Add(p);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(p.Id);
    }

    public async Task<PartDto> UpdateAsync(Guid id, UpdatePartRequest request)
    {
        var p = await _db.Parts.FindAsync(id) ?? throw new KeyNotFoundException("Part not found.");
        p.Name = request.Name;
        p.Category = request.Category;
        p.Description = request.Description;
        p.PurchasePrice = request.PurchasePrice;
        p.SellingPrice = request.SellingPrice;
        p.StockQuantity = request.StockQuantity;
        p.LowStockThreshold = request.LowStockThreshold;
        p.CompatibleMake = request.CompatibleMake;
        p.CompatibleModel = request.CompatibleModel;
        p.ImageUrl = request.ImageUrl;
        p.VendorId = request.VendorId;
        p.IsActive = request.IsActive;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var p = await _db.Parts.FindAsync(id) ?? throw new KeyNotFoundException("Part not found.");
        _db.Parts.Remove(p);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<PartDto>> GetLowStockAsync()
    {
        return await _db.Parts.Include(p => p.Vendor)
            .Where(p => p.StockQuantity < p.LowStockThreshold && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .Select(p => Map(p))
            .ToListAsync();
    }
}
