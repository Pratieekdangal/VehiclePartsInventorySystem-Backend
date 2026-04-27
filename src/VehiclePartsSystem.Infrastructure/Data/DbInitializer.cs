using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Domain.Entities;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            db.Users.Add(new User
            {
                FullName = "System Administrator",
                Email = "admin@vps.local",
                PhoneNumber = "+9779800000000",
                Role = UserRole.Admin,
                Address = "Kathmandu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123")
            });
            await db.SaveChangesAsync();
        }
    }
}
