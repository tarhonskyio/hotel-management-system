using FluentValidation;
using HotelManagement.Application.Parking;
using HotelManagement.Application.Reservations;
using HotelManagement.Application.Reservations.Validators;
using HotelManagement.Application.Rooms;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.Application;

/// <summary>
/// Registers application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services and validators.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IParkingService, ParkingService>();
        services.AddValidatorsFromAssemblyContaining<CreateReservationDtoValidator>();

        return services;
    }
}
