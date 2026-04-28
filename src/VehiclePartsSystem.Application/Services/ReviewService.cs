using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;

namespace VehiclePartsSystem.Application.Services;

public interface IReviewService
{
    Task<IEnumerable<ReviewDto>> GetVisibleAsync();
    Task<IEnumerable<ReviewDto>> GetAllAsync();
    Task<ReviewDto> CreateAsync(Guid customerId, CreateReviewRequest request);
    Task<ReviewDto> SetVisibilityAsync(Guid id, bool visible);
    Task DeleteAsync(Guid id);
}

public class ReviewService : IReviewService
{
    private readonly IAppDbContext _db;
    public ReviewService(IAppDbContext db) => _db = db;

    private static ReviewDto Map(Review r) => new(
        r.Id, r.CustomerId, r.Customer.FullName, r.Rating, r.Comment, r.IsVisible, r.CreatedAt);

    public async Task<IEnumerable<ReviewDto>> GetVisibleAsync()
    {
        var rows = await _db.Reviews.Include(r => r.Customer)
            .Where(r => r.IsVisible)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return rows.Select(Map);
    }

    public async Task<IEnumerable<ReviewDto>> GetAllAsync()
    {
        var rows = await _db.Reviews.Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return rows.Select(Map);
    }

    public async Task<ReviewDto> CreateAsync(Guid customerId, CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
            throw new InvalidOperationException("Rating must be between 1 and 5.");

        var r = new Review { CustomerId = customerId, Rating = request.Rating, Comment = request.Comment };
        _db.Reviews.Add(r);
        await _db.SaveChangesAsync();
        var saved = await _db.Reviews.Include(x => x.Customer).FirstAsync(x => x.Id == r.Id);
        return Map(saved);
    }

    public async Task<ReviewDto> SetVisibilityAsync(Guid id, bool visible)
    {
        var r = await _db.Reviews.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Review not found.");
        r.IsVisible = visible;
        await _db.SaveChangesAsync();
        return Map(r);
    }

    public async Task DeleteAsync(Guid id)
    {
        var r = await _db.Reviews.FindAsync(id) ?? throw new KeyNotFoundException("Review not found.");
        _db.Reviews.Remove(r);
        await _db.SaveChangesAsync();
    }
}
