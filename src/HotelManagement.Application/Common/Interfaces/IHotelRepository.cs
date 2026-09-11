using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Common.Interfaces;

public interface IHotelRepository
{
    Task<IReadOnlyList<Room>> GetRoomsAsync(CancellationToken cancellationToken);

    Task<Room?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Room?> GetRoomByNumberAsync(string number, CancellationToken cancellationToken);

    Task AddRoomAsync(Room room, CancellationToken cancellationToken);

    void RemoveRoom(Room room);

    Task<IReadOnlyList<Reservation>> GetReservationsAsync(CancellationToken cancellationToken);

    Task<Reservation?> GetReservationByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddReservationAsync(Reservation reservation, CancellationToken cancellationToken);

    Task<bool> HasRoomOverlapAsync(Guid roomId, DateTime checkIn, DateTime checkOut, Guid? excludedReservationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Vehicle>> GetVehiclesForReservationAsync(Guid reservationId, CancellationToken cancellationToken);

    Task<Vehicle?> GetVehicleByRegistrationAsync(string registrationNumber, CancellationToken cancellationToken);

    Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken);

    Task AddParkingLogAsync(ParkingLog parkingLog, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
