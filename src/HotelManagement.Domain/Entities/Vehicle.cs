namespace HotelManagement.Domain.Entities;

public sealed class Vehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string RegistrationNumber { get; set; } = string.Empty;

    public Guid ReservationId { get; set; }

    public Reservation? Reservation { get; set; }

    public DateTime ParkingAccessFrom { get; set; }

    public DateTime ParkingAccessTo { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ParkingLog> ParkingLogs { get; set; } = new List<ParkingLog>();
}
