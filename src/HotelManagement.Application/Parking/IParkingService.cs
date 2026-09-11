using HotelManagement.Application.Parking.Dtos;

namespace HotelManagement.Application.Parking;

public interface IParkingService
{
    Task<ParkingAccessResponseDto> AssignVehicleAsync(Guid reservationId, AssignVehicleDto dto, CancellationToken cancellationToken);

    Task<IReadOnlyList<ParkingAccessResponseDto>> GetReservationParkingAccessAsync(Guid reservationId, CancellationToken cancellationToken);

    Task<ParkingVerificationResponseDto> VerifyAccessAsync(string registrationNumber, CancellationToken cancellationToken);
}
