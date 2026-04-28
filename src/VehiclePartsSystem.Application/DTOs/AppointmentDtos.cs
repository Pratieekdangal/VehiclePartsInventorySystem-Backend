using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Application.DTOs;

public record AppointmentDto(
    Guid Id, Guid CustomerId, string CustomerName,
    Guid? VehicleId, string? VehicleNumber,
    DateTime AppointmentDate, string ServiceType,
    string? Description, AppointmentStatus Status, DateTime CreatedAt
);

public record CreateAppointmentRequest(
    Guid? VehicleId, DateTime AppointmentDate, string ServiceType, string? Description
);

public record UpdateAppointmentStatusRequest(AppointmentStatus Status);

public record ReviewDto(
    Guid Id, Guid CustomerId, string CustomerName,
    int Rating, string Comment, bool IsVisible, DateTime CreatedAt
);

public record CreateReviewRequest(int Rating, string Comment);

public record PartRequestDto(
    Guid Id, Guid CustomerId, string CustomerName,
    string PartName, string? Description, string? CompatibleVehicle,
    PartRequestStatus Status, string? AdminResponse, DateTime CreatedAt
);

public record CreatePartRequestRequest(
    string PartName, string? Description, string? CompatibleVehicle
);

public record RespondToPartRequestRequest(PartRequestStatus Status, string? AdminResponse);
