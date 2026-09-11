using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles", table =>
        {
            table.HasCheckConstraint("ck_vehicles_registration_number_not_empty", "length(trim(registration_number)) > 0");
            table.HasCheckConstraint("ck_vehicles_access_to_after_access_from", "parking_access_to > parking_access_from");
        });
        builder.HasKey(vehicle => vehicle.Id);

        builder.Property(vehicle => vehicle.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(vehicle => vehicle.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(15).IsRequired();
        builder.Property(vehicle => vehicle.ReservationId).HasColumnName("reservation_id").HasColumnType("uuid").IsRequired();
        builder.Property(vehicle => vehicle.ParkingAccessFrom).HasColumnName("parking_access_from").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(vehicle => vehicle.ParkingAccessTo).HasColumnName("parking_access_to").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(vehicle => vehicle.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasOne(vehicle => vehicle.Reservation)
            .WithMany(reservation => reservation.Vehicles)
            .HasForeignKey(vehicle => vehicle.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(vehicle => vehicle.RegistrationNumber).IsUnique().HasDatabaseName("ux_vehicles_registration_number");
        builder.HasIndex(vehicle => new { vehicle.ReservationId, vehicle.IsActive }).HasDatabaseName("ix_vehicles_reservation_active");
    }
}
