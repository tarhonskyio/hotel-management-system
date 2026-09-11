using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Parking.Dtos;

public sealed class ParkingVerificationResponseDto
{
    public string RegistrationNumber { get; init; } = string.Empty;

    public bool HasValidAccess { get; init; }

    public ParkingVerificationStatus VerificationStatus { get; init; }

    public Guid? VehicleId { get; init; }

    public Guid? ReservationId { get; init; }
}
