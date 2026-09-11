using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Rooms.Dtos;

public sealed class RoomResponseDto
{
    public Guid Id { get; init; }

    public string Number { get; init; } = string.Empty;

    public RoomType Type { get; init; }

    public int Capacity { get; init; }

    public RoomStatus Status { get; init; }
}
