namespace VehiclePartsSystem.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Overdue = 4
}

public enum PartRequestStatus
{
    Pending = 1,
    Fulfilled = 2,
    Rejected = 3
}
