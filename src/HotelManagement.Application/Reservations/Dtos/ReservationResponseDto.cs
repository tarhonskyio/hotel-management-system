using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Reservations.Dtos;

/// <summary>
/// Response payload returned for reservation API operations.
/// </summary>
public sealed class ReservationResponseDto
{
    /// <summary>
    /// Unique reservation identifier.
    /// </summary>
    public Guid Id { get; init; }

    public string GuestFirstName { get; init; } = string.Empty;

    public string GuestLastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public DateTime CheckIn { get; init; }

    public DateTime CheckOut { get; init; }

    public Guid RoomId { get; init; }

    public string RoomNumber { get; init; } = string.Empty;

    public ReservationStatus Status { get; init; }

    public IReadOnlyList<string> VehicleRegistrationNumbers { get; init; } = Array.Empty<string>();
}
