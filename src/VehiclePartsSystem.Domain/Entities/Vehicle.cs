using VehiclePartsSystem.Domain.Common;

namespace VehiclePartsSystem.Domain.Entities;

public class Vehicle : BaseEntity
{
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public string VehicleNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? Color { get; set; }
    public int Mileage { get; set; }
    public DateTime? LastServiceDate { get; set; }
}
