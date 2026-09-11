using HotelManagement.Application.Reservations.Dtos;

namespace HotelManagement.Application.Reservations;

/// <summary>
/// Provides reservation business operations.
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// Creates a new reservation.
    /// </summary>
    Task<ReservationResponseDto> CreateAsync(CreateReservationDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all active reservations.
    /// </summary>
    Task<IReadOnlyList<ReservationResponseDto>> GetActiveAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a reservation by identifier.
    /// </summary>
    Task<ReservationResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a reservation.
    /// </summary>
    Task<ReservationResponseDto> UpdateAsync(Guid id, UpdateReservationDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels a reservation using a soft delete.
    /// </summary>
    Task CancelAsync(Guid id, CancellationToken cancellationToken);
}
