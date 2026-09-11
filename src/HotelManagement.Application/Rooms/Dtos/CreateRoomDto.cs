using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Rooms.Dtos;

public sealed class CreateRoomDto
{
    [Required]
    [MaxLength(20)]
    public string Number { get; init; } = string.Empty;

    [Required]
    public RoomType Type { get; init; }

    [Range(1, 20)]
    public int Capacity { get; init; }

    public RoomStatus Status { get; init; } = RoomStatus.Available;
}
