using VehiclePartsSystem.Domain.Common;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;
    public Guid? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime AppointmentDate { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
}
