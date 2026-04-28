using VehiclePartsSystem.Domain.Common;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Domain.Entities;

public class PartRequest : BaseEntity
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public string PartName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CompatibleVehicle { get; set; }
    public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;
    public string? AdminResponse { get; set; }
}
