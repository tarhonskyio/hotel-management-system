using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Parking;

public static class ParkingAccessEvaluator
{
    public static ParkingVerificationStatus Evaluate(Vehicle? vehicle, DateTime utcNow)
    {
        if (vehicle is null)
        {
            return ParkingVerificationStatus.UnknownVehicle;
        }

        if (!vehicle.IsActive || utcNow < vehicle.ParkingAccessFrom || utcNow > vehicle.ParkingAccessTo)
        {
            return ParkingVerificationStatus.Expired;
        }

        return ParkingVerificationStatus.Allowed;
    }
}
