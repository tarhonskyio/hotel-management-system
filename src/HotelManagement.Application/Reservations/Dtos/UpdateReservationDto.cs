using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Reservations.Dtos;

/// <summary>
/// Request payload used to update an existing reservation.
/// </summary>
public sealed class UpdateReservationDto
{
    [MaxLength(80)]
    public string? GuestFirstName { get; init; }

    [MaxLength(80)]
    public string? GuestLastName { get; init; }

    [EmailAddress]
    [MaxLength(160)]
    public string? Email { get; init; }

    public DateTime? CheckIn { get; init; }

    public DateTime? CheckOut { get; init; }

    public Guid? RoomId { get; init; }

    public ReservationStatus? Status { get; init; }
}
