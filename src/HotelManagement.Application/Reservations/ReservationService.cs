using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Reservations.Dtos;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Reservations;

public sealed class ReservationService : IReservationService
{
    private readonly IHotelRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReservationService(IHotelRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ReservationResponseDto> CreateAsync(CreateReservationDto dto, CancellationToken cancellationToken)
    {
        var checkIn = dto.CheckIn.Date;
        var checkOut = dto.CheckOut.Date;
        ValidateDates(checkIn, checkOut, preventPastCheckIn: true);

        var room = await _repository.GetRoomByIdAsync(dto.RoomId, cancellationToken);
        if (room is null)
        {
            throw new NotFoundException($"Room '{dto.RoomId}' was not found.");
        }

        await EnsureRoomAvailableAsync(dto.RoomId, checkIn, checkOut, null, cancellationToken);

        var reservation = new Reservation
        {
            GuestFirstName = dto.GuestFirstName.Trim(),
            GuestLastName = dto.GuestLastName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            CheckIn = checkIn,
            CheckOut = checkOut,
            RoomId = dto.RoomId,
            Room = room,
            Status = dto.Status,
            CreatedAtUtc = _dateTimeProvider.UtcNow
        };

        await _repository.AddReservationAsync(reservation, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return reservation.ToResponseDto();
    }

    public async Task<IReadOnlyList<ReservationResponseDto>> GetActiveAsync(CancellationToken cancellationToken)
    {
        var reservations = await _repository.GetReservationsAsync(cancellationToken);
        return reservations
            .Where(reservation => reservation.Status != ReservationStatus.Cancelled)
            .Select(reservation => reservation.ToResponseDto())
            .ToList();
    }

    public async Task<ReservationResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await GetReservationOrThrowAsync(id, cancellationToken);
        return reservation.ToResponseDto();
    }

    public async Task<ReservationResponseDto> UpdateAsync(Guid id, UpdateReservationDto dto, CancellationToken cancellationToken)
    {
        var reservation = await GetReservationOrThrowAsync(id, cancellationToken);
        var roomId = dto.RoomId ?? reservation.RoomId;
        var checkIn = dto.CheckIn?.Date ?? reservation.CheckIn.Date;
        var checkOut = dto.CheckOut?.Date ?? reservation.CheckOut.Date;

        ValidateDates(checkIn, checkOut, preventPastCheckIn: dto.CheckIn.HasValue);

        if (dto.RoomId.HasValue && await _repository.GetRoomByIdAsync(roomId, cancellationToken) is null)
        {
            throw new NotFoundException($"Room '{roomId}' was not found.");
        }

        await EnsureRoomAvailableAsync(roomId, checkIn, checkOut, reservation.Id, cancellationToken);

        reservation.GuestFirstName = dto.GuestFirstName?.Trim() ?? reservation.GuestFirstName;
        reservation.GuestLastName = dto.GuestLastName?.Trim() ?? reservation.GuestLastName;
        reservation.Email = dto.Email?.Trim().ToLowerInvariant() ?? reservation.Email;
        reservation.CheckIn = checkIn;
        reservation.CheckOut = checkOut;
        reservation.RoomId = roomId;
        reservation.Status = dto.Status ?? reservation.Status;
        reservation.UpdatedAtUtc = _dateTimeProvider.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return reservation.ToResponseDto();
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await GetReservationOrThrowAsync(id, cancellationToken);
        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAtUtc = _dateTimeProvider.UtcNow;

        foreach (var vehicle in reservation.Vehicles)
        {
            vehicle.IsActive = false;
        }

        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Reservation> GetReservationOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _repository.GetReservationByIdAsync(id, cancellationToken);
        return reservation ?? throw new NotFoundException($"Reservation '{id}' was not found.");
    }

    private async Task EnsureRoomAvailableAsync(
        Guid roomId,
        DateTime checkIn,
        DateTime checkOut,
        Guid? excludedReservationId,
        CancellationToken cancellationToken)
    {
        var hasOverlap = await _repository.HasRoomOverlapAsync(roomId, checkIn, checkOut, excludedReservationId, cancellationToken);
        if (hasOverlap)
        {
            throw new ConflictException("The selected room already has an active reservation in this date range.");
        }
    }

    private void ValidateDates(DateTime checkIn, DateTime checkOut, bool preventPastCheckIn)
    {
        if (checkOut <= checkIn)
        {
            throw new BusinessRuleException("Check-out date must be after check-in date.");
        }

        if (preventPastCheckIn && checkIn < _dateTimeProvider.UtcNow.Date)
        {
            throw new BusinessRuleException("Check-in date cannot be in the past.");
        }
    }
}
