using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Persistence;

public sealed class HotelRepository : IHotelRepository
{
    private readonly ApplicationDbContext _dbContext;

    public HotelRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Room>> GetRoomsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms
            .AsNoTracking()
            .OrderBy(room => room.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<Room?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms.FirstOrDefaultAsync(room => room.Id == id, cancellationToken);
    }

    public async Task<Room?> GetRoomByNumberAsync(string number, CancellationToken cancellationToken)
    {
        return await _dbContext.Rooms.FirstOrDefaultAsync(room => room.Number == number, cancellationToken);
    }

    public async Task AddRoomAsync(Room room, CancellationToken cancellationToken)
    {
        await _dbContext.Rooms.AddAsync(room, cancellationToken);
    }

    public void RemoveRoom(Room room)
    {
        _dbContext.Rooms.Remove(room);
    }

    public async Task<IReadOnlyList<Reservation>> GetReservationsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .Include(reservation => reservation.Room)
            .Include(reservation => reservation.Vehicles)
            .OrderBy(reservation => reservation.CheckIn)
            .ThenBy(reservation => reservation.Room!.Number)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reservation?> GetReservationByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .Include(reservation => reservation.Room)
            .Include(reservation => reservation.Vehicles)
            .FirstOrDefaultAsync(reservation => reservation.Id == id, cancellationToken);
    }

    public async Task AddReservationAsync(Reservation reservation, CancellationToken cancellationToken)
    {
        await _dbContext.Reservations.AddAsync(reservation, cancellationToken);
    }

    public async Task<bool> HasRoomOverlapAsync(
        Guid roomId,
        DateTime checkIn,
        DateTime checkOut,
        Guid? excludedReservationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservations
            .AsNoTracking()
            .AnyAsync(
                reservation =>
                    reservation.RoomId == roomId
                    && reservation.Status != ReservationStatus.Cancelled
                    && reservation.Status != ReservationStatus.CheckedOut
                    && (excludedReservationId == null || reservation.Id != excludedReservationId)
                    && reservation.CheckIn < checkOut
                    && checkIn < reservation.CheckOut,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesForReservationAsync(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Vehicles
            .AsNoTracking()
            .Where(vehicle => vehicle.ReservationId == reservationId)
            .OrderBy(vehicle => vehicle.RegistrationNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Vehicle?> GetVehicleByRegistrationAsync(
        string registrationNumber,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Vehicles
            .FirstOrDefaultAsync(vehicle => vehicle.RegistrationNumber == registrationNumber, cancellationToken);
    }

    public async Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        await _dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
    }

    public async Task AddParkingLogAsync(ParkingLog parkingLog, CancellationToken cancellationToken)
    {
        await _dbContext.ParkingLogs.AddAsync(parkingLog, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
