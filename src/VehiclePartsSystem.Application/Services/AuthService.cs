using Microsoft.EntityFrameworkCore;
using VehiclePartsSystem.Application.Common.Interfaces;
using VehiclePartsSystem.Application.DTOs;
using VehiclePartsSystem.Domain.Entities;
using VehiclePartsSystem.Domain.Enums;
using BCrypt.Net;

namespace VehiclePartsSystem.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterCustomerAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthService(IAppDbContext db, IJwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterCustomerAsync(RegisterRequest request)
    {
        var emailTaken = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new AuthResponse(_jwt.GenerateToken(user), user.Id, user.FullName, user.Email, user.Role.ToString());
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return new AuthResponse(_jwt.GenerateToken(user), user.Id, user.FullName, user.Email, user.Role.ToString());
    }
}
