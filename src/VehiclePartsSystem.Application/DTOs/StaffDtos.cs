using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.DTOs;

public record StaffDto(
    Guid Id, string FullName, string Email, string PhoneNumber,
    UserRole Role, bool IsActive, DateTime CreatedAt
);

public record CreateStaffRequest(
    string FullName, string Email, string PhoneNumber, string Password, string? Address
);

public record UpdateStaffRequest(
    string FullName, string PhoneNumber, string? Address, bool IsActive
);
