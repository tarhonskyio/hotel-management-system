using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

public sealed class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Number { get; set; } = string.Empty;

    public RoomType Type { get; set; }

    public int Capacity { get; set; }

    public RoomStatus Status { get; set; } = RoomStatus.Available;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
