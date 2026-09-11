using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the hotel management system.
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Reservation records.
    /// </summary>
    public DbSet<Reservation> Reservations => Set<Reservation>();

    /// <summary>
    /// Hotel room records.
    /// </summary>
    public DbSet<Room> Rooms => Set<Room>();

    /// <summary>
    /// Vehicle parking access records.
    /// </summary>
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    /// <summary>
    /// Employee user records.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Parking access verification logs.
    /// </summary>
    public DbSet<ParkingLog> ParkingLogs => Set<ParkingLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
