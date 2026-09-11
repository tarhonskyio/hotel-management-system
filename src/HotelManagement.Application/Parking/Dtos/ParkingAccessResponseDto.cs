namespace HotelManagement.Application.Parking.Dtos;

public sealed class ParkingAccessResponseDto
{
    public Guid VehicleId { get; init; }

    public Guid ReservationId { get; init; }

    public string RegistrationNumber { get; init; } = string.Empty;

    public DateTime ParkingAccessFrom { get; init; }

    public DateTime ParkingAccessTo { get; init; }

    public bool IsActive { get; init; }

    public bool HasValidAccessNow { get; init; }
}
