using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Rooms.Dtos;

public sealed class UpdateRoomDto
{
    [MaxLength(20)]
    public string? Number { get; init; }

    public RoomType? Type { get; init; }

    [Range(1, 20)]
    public int? Capacity { get; init; }

    public RoomStatus? Status { get; init; }
}
