using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms", table =>
        {
            table.HasCheckConstraint("ck_rooms_capacity_positive", "capacity > 0");
            table.HasCheckConstraint("ck_rooms_number_not_empty", "length(trim(number)) > 0");
        });
        builder.HasKey(room => room.Id);

        builder.Property(room => room.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(room => room.Number).HasColumnName("number").HasMaxLength(20).IsRequired();
        builder.Property(room => room.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(room => room.Capacity).HasColumnName("capacity").IsRequired();
        builder.Property(room => room.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasIndex(room => room.Number).IsUnique().HasDatabaseName("ux_rooms_number");
    }
}
