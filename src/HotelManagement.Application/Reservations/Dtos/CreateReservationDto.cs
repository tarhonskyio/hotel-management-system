using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Reservations.Dtos;

/// <summary>
/// Request payload used to create a hotel reservation.
/// </summary>
public sealed class CreateReservationDto
{
    [Required]
    [MaxLength(80)]
    public string GuestFirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string GuestLastName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(160)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public DateTime CheckIn { get; init; }

    [Required]
    public DateTime CheckOut { get; init; }

    [Required]
    public Guid RoomId { get; init; }

    public ReservationStatus Status { get; init; } = ReservationStatus.Confirmed;
}
