using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

public sealed class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string GuestFirstName { get; set; } = string.Empty;

    public string GuestLastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public Guid RoomId { get; set; }

    public Room? Room { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
