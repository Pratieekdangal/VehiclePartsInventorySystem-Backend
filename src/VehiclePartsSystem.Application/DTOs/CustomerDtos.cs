namespace VehiclePartsSystem.Application.DTOs;

public record VehicleDto(
    Guid Id, Guid CustomerId, string VehicleNumber,
    string Make, string Model, int Year, string? Color, int Mileage,
    DateTime? LastServiceDate, DateTime CreatedAt
);

public record CreateVehicleRequest(
    string VehicleNumber, string Make, string Model, int Year,
    string? Color, int Mileage
);

public record UpdateVehicleRequest(
    string VehicleNumber, string Make, string Model, int Year,
    string? Color, int Mileage, DateTime? LastServiceDate
);

public record CustomerSummaryDto(
    Guid Id, string FullName, string Email, string PhoneNumber,
    string? Address, bool IsActive, int VehicleCount, decimal TotalSpent,
    decimal PendingCredit, DateTime CreatedAt
);

public record CustomerDetailDto(
    Guid Id, string FullName, string Email, string PhoneNumber,
    string? Address, bool IsActive, IEnumerable<VehicleDto> Vehicles,
    decimal TotalSpent, decimal PendingCredit, int InvoiceCount, DateTime CreatedAt
);

public record CreateCustomerByStaffRequest(
    string FullName, string Email, string PhoneNumber, string? Address,
    string Password, IEnumerable<CreateVehicleRequest>? Vehicles
);

public record UpdateProfileRequest(
    string FullName, string PhoneNumber, string? Address
);
