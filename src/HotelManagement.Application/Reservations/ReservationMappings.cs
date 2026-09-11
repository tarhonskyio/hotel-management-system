using HotelManagement.Application.Reservations.Dtos;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Reservations;

internal static class ReservationMappings
{
    public static ReservationResponseDto ToResponseDto(this Reservation reservation)
    {
        return new ReservationResponseDto
        {
            Id = reservation.Id,
            GuestFirstName = reservation.GuestFirstName,
            GuestLastName = reservation.GuestLastName,
            Email = reservation.Email,
            CheckIn = reservation.CheckIn,
            CheckOut = reservation.CheckOut,
            RoomId = reservation.RoomId,
            RoomNumber = reservation.Room?.Number ?? string.Empty,
            Status = reservation.Status,
            VehicleRegistrationNumbers = reservation.Vehicles
                .Where(vehicle => vehicle.IsActive)
                .Select(vehicle => vehicle.RegistrationNumber)
                .ToList()
        };
    }
}
