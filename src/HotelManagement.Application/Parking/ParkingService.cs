using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Parking.Dtos;
using HotelManagement.Application.Reservations;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Parking;

public sealed class ParkingService : IParkingService
{
    private readonly IHotelRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ParkingService(IHotelRepository repository, IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ParkingAccessResponseDto> AssignVehicleAsync(
        Guid reservationId,
        AssignVehicleDto dto,
        CancellationToken cancellationToken)
    {
        var reservation = await _repository.GetReservationByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            throw new NotFoundException($"Reservation '{reservationId}' was not found.");
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new BusinessRuleException("Cannot assign parking access to a cancelled reservation.");
        }

        var registrationNumber = NormalizeRegistrationNumberOrThrow(dto.RegistrationNumber);
        var existingVehicle = await _repository.GetVehicleByRegistrationAsync(registrationNumber, cancellationToken);
        if (existingVehicle is not null && existingVehicle.ReservationId != reservationId && existingVehicle.IsActive)
        {
            throw new ConflictException($"Vehicle '{registrationNumber}' already has active parking access.");
        }

        var vehicle = existingVehicle ?? new Vehicle
        {
            RegistrationNumber = registrationNumber
        };

        vehicle.ReservationId = reservationId;
        vehicle.ParkingAccessFrom = reservation.CheckIn;
        vehicle.ParkingAccessTo = reservation.CheckOut.AddDays(1).AddTicks(-1);
        vehicle.IsActive = true;

        if (existingVehicle is null)
        {
            await _repository.AddVehicleAsync(vehicle, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return ToParkingAccessResponse(vehicle, _dateTimeProvider.UtcNow);
    }

    public async Task<IReadOnlyList<ParkingAccessResponseDto>> GetReservationParkingAccessAsync(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        if (await _repository.GetReservationByIdAsync(reservationId, cancellationToken) is null)
        {
            throw new NotFoundException($"Reservation '{reservationId}' was not found.");
        }

        var vehicles = await _repository.GetVehiclesForReservationAsync(reservationId, cancellationToken);
        return vehicles
            .Select(vehicle => ToParkingAccessResponse(vehicle, _dateTimeProvider.UtcNow))
            .ToList();
    }

    public async Task<ParkingVerificationResponseDto> VerifyAccessAsync(
        string registrationNumber,
        CancellationToken cancellationToken)
    {
        var normalizedRegistrationNumber = NormalizeRegistrationNumberOrThrow(registrationNumber);
        var vehicle = await _repository.GetVehicleByRegistrationAsync(normalizedRegistrationNumber, cancellationToken);
        var status = ParkingAccessEvaluator.Evaluate(vehicle, _dateTimeProvider.UtcNow);

        var parkingLog = new ParkingLog
        {
            VehicleId = vehicle?.Id,
            RegistrationNumber = normalizedRegistrationNumber,
            Timestamp = _dateTimeProvider.UtcNow,
            EventType = ParkingEventType.Entry,
            VerificationStatus = status
        };

        await _repository.AddParkingLogAsync(parkingLog, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return new ParkingVerificationResponseDto
        {
            RegistrationNumber = normalizedRegistrationNumber,
            HasValidAccess = status == ParkingVerificationStatus.Allowed,
            VerificationStatus = status,
            VehicleId = vehicle?.Id,
            ReservationId = vehicle?.ReservationId
        };
    }

    private static ParkingAccessResponseDto ToParkingAccessResponse(Vehicle vehicle, DateTime utcNow)
    {
        return new ParkingAccessResponseDto
        {
            VehicleId = vehicle.Id,
            ReservationId = vehicle.ReservationId,
            RegistrationNumber = vehicle.RegistrationNumber,
            ParkingAccessFrom = vehicle.ParkingAccessFrom,
            ParkingAccessTo = vehicle.ParkingAccessTo,
            IsActive = vehicle.IsActive,
            HasValidAccessNow = ParkingAccessEvaluator.Evaluate(vehicle, utcNow) == ParkingVerificationStatus.Allowed
        };
    }

    private static string NormalizeRegistrationNumberOrThrow(string registrationNumber)
    {
        var normalized = LicensePlateFormatter.Normalize(registrationNumber);
        if (!LicensePlateFormatter.IsNormalizedValid(normalized))
        {
            throw new BusinessRuleException("Registration number must contain 2 to 15 letters or digits.");
        }

        return normalized!;
    }
}
