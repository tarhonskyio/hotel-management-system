using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public sealed class ParkingLogConfiguration : IEntityTypeConfiguration<ParkingLog>
{
    public void Configure(EntityTypeBuilder<ParkingLog> builder)
    {
        builder.ToTable("parking_logs");
        builder.HasKey(log => log.Id);

        builder.Property(log => log.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(log => log.VehicleId).HasColumnName("vehicle_id").HasColumnType("uuid");
        builder.Property(log => log.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(15).IsRequired();
        builder.Property(log => log.Timestamp).HasColumnName("timestamp").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(log => log.EventType).HasColumnName("event_type").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(log => log.VerificationStatus).HasColumnName("verification_status").HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasOne(log => log.Vehicle)
            .WithMany(vehicle => vehicle.ParkingLogs)
            .HasForeignKey(log => log.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(log => new { log.RegistrationNumber, log.Timestamp }).HasDatabaseName("ix_parking_logs_registration_timestamp");
        builder.HasIndex(log => log.VehicleId).HasDatabaseName("ix_parking_logs_vehicle_id");
    }
}
