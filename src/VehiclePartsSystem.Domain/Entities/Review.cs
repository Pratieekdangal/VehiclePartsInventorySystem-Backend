using VehiclePartsSystem.Domain.Common;

namespace VehiclePartsSystem.Domain.Entities;

public class Review : BaseEntity
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
}
