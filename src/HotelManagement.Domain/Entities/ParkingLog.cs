using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

public sealed class ParkingLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? VehicleId { get; set; }

    public Vehicle? Vehicle { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ParkingEventType EventType { get; set; }

    public ParkingVerificationStatus VerificationStatus { get; set; }
}
