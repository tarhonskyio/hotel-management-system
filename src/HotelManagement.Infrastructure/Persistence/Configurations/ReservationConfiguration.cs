using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");
        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(reservation => reservation.GuestFirstName).HasColumnName("guest_first_name").HasMaxLength(80).IsRequired();
        builder.Property(reservation => reservation.GuestLastName).HasColumnName("guest_last_name").HasMaxLength(80).IsRequired();
        builder.Property(reservation => reservation.Email).HasColumnName("email").HasMaxLength(160).IsRequired();
        builder.Property(reservation => reservation.CheckIn).HasColumnName("check_in").HasColumnType("date").IsRequired();
        builder.Property(reservation => reservation.CheckOut).HasColumnName("check_out").HasColumnType("date").IsRequired();
        builder.Property(reservation => reservation.RoomId).HasColumnName("room_id").HasColumnType("uuid").IsRequired();
        builder.Property(reservation => reservation.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(reservation => reservation.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(reservation => reservation.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone");

        builder.HasOne(reservation => reservation.Room)
            .WithMany(room => room.Reservations)
            .HasForeignKey(reservation => reservation.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(reservation => new { reservation.RoomId, reservation.CheckIn, reservation.CheckOut })
            .HasDatabaseName("ix_reservations_room_dates");

        builder.HasIndex(reservation => reservation.Email)
            .HasDatabaseName("ix_reservations_email");
    }
}
