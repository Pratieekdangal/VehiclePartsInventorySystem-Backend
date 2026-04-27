using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.DTOs;

public record RegisterRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    string? Address
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    Guid UserId,
    string FullName,
    string Email,
    string Role
);

public record CreateUserByAdminRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Password,
    UserRole Role,
    string? Address
);
