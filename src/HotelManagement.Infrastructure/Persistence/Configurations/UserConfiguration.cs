using HotelManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelManagement.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("ck_users_username_not_empty", "length(trim(username)) > 0");
            table.HasCheckConstraint("ck_users_email_not_empty", "length(trim(email)) > 0");
            table.HasCheckConstraint("ck_users_password_hash_not_empty", "length(trim(password_hash)) > 0");
        });
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(user => user.Username).HasColumnName("username").HasMaxLength(80).IsRequired();
        builder.Property(user => user.Email).HasColumnName("email").HasMaxLength(160).IsRequired();
        builder.Property(user => user.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
        builder.Property(user => user.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasIndex(user => user.Username).IsUnique().HasDatabaseName("ux_users_username");
        builder.HasIndex(user => user.Email).IsUnique().HasDatabaseName("ux_users_email");
    }
}
