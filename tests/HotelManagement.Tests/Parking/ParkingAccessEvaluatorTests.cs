using HotelManagement.Application.Parking;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Tests.Parking;

public sealed class ParkingAccessEvaluatorTests
{
    [Fact]
    public void Evaluate_ReturnsAllowed_WhenVehicleIsActiveAndWithinAccessWindow()
    {
        var utcNow = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);
        var vehicle = new Vehicle
        {
            RegistrationNumber = "AA1234BB",
            IsActive = true,
            ParkingAccessFrom = utcNow.AddHours(-1),
            ParkingAccessTo = utcNow.AddHours(1)
        };

        var result = ParkingAccessEvaluator.Evaluate(vehicle, utcNow);

        Assert.Equal(ParkingVerificationStatus.Allowed, result);
    }

    [Fact]
    public void Evaluate_ReturnsUnknownVehicle_WhenVehicleDoesNotExist()
    {
        var result = ParkingAccessEvaluator.Evaluate(null, DateTime.UtcNow);

        Assert.Equal(ParkingVerificationStatus.UnknownVehicle, result);
    }

    [Fact]
    public void Evaluate_ReturnsExpired_WhenVehicleAccessWindowIsNotCurrent()
    {
        var utcNow = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);
        var vehicle = new Vehicle
        {
            RegistrationNumber = "AA1234BB",
            IsActive = true,
            ParkingAccessFrom = utcNow.AddDays(-2),
            ParkingAccessTo = utcNow.AddDays(-1)
        };

        var result = ParkingAccessEvaluator.Evaluate(vehicle, utcNow);

        Assert.Equal(ParkingVerificationStatus.Expired, result);
    }
}
