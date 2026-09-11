using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "ck_vehicles_access_to_after_access_from",
                table: "vehicles",
                sql: "parking_access_to > parking_access_from");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vehicles_registration_number_not_empty",
                table: "vehicles",
                sql: "length(trim(registration_number)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_email_not_empty",
                table: "users",
                sql: "length(trim(email)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_password_hash_not_empty",
                table: "users",
                sql: "length(trim(password_hash)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_users_username_not_empty",
                table: "users",
                sql: "length(trim(username)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_rooms_capacity_positive",
                table: "rooms",
                sql: "capacity > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_rooms_number_not_empty",
                table: "rooms",
                sql: "length(trim(number)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_reservations_check_out_after_check_in",
                table: "reservations",
                sql: "check_out > check_in");

            migrationBuilder.AddCheckConstraint(
                name: "ck_reservations_email_not_empty",
                table: "reservations",
                sql: "length(trim(email)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_reservations_guest_first_name_not_empty",
                table: "reservations",
                sql: "length(trim(guest_first_name)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_reservations_guest_last_name_not_empty",
                table: "reservations",
                sql: "length(trim(guest_last_name)) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_parking_logs_registration_number_not_empty",
                table: "parking_logs",
                sql: "length(trim(registration_number)) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_vehicles_access_to_after_access_from",
                table: "vehicles");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vehicles_registration_number_not_empty",
                table: "vehicles");

            migrationBuilder.DropCheckConstraint(
                name: "ck_users_email_not_empty",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "ck_users_password_hash_not_empty",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "ck_users_username_not_empty",
                table: "users");

            migrationBuilder.DropCheckConstraint(
                name: "ck_rooms_capacity_positive",
                table: "rooms");

            migrationBuilder.DropCheckConstraint(
                name: "ck_rooms_number_not_empty",
                table: "rooms");

            migrationBuilder.DropCheckConstraint(
                name: "ck_reservations_check_out_after_check_in",
                table: "reservations");

            migrationBuilder.DropCheckConstraint(
                name: "ck_reservations_email_not_empty",
                table: "reservations");

            migrationBuilder.DropCheckConstraint(
                name: "ck_reservations_guest_first_name_not_empty",
                table: "reservations");

            migrationBuilder.DropCheckConstraint(
                name: "ck_reservations_guest_last_name_not_empty",
                table: "reservations");

            migrationBuilder.DropCheckConstraint(
                name: "ck_parking_logs_registration_number_not_empty",
                table: "parking_logs");
        }
    }
}
