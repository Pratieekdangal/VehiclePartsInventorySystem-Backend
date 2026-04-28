namespace VehiclePartsSystem.Application.DTOs;

public record VendorDto(
    Guid Id, string Name, string ContactPerson, string Email,
    string PhoneNumber, string Address, bool IsActive, DateTime CreatedAt
);

public record CreateVendorRequest(
    string Name, string ContactPerson, string Email,
    string PhoneNumber, string Address
);

public record UpdateVendorRequest(
    string Name, string ContactPerson, string Email,
    string PhoneNumber, string Address, bool IsActive
);
