using VehiclePartsSystem.Domain.Common;
using VehiclePartsSystem.Domain.Enums;

namespace VehiclePartsSystem.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<SalesInvoice> Purchases { get; set; } = new List<SalesInvoice>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();
}
