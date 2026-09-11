# EPIC 1 — Hotel management and database schema

This document describes the PostgreSQL relational schema prepared for the Hotel Management System MVP.

## Tables

### rooms

Stores fixed hotel rooms.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| id | uuid | yes | Primary key |
| number | varchar(20) | yes | Unique room number |
| type | varchar(30) | yes | Room type enum |
| capacity | integer | yes | Must be greater than zero |
| status | varchar(30) | yes | Room status enum |

Indexes and constraints:

- `pk_rooms`
- `ux_rooms_number`
- `ck_rooms_capacity_positive`
- `ck_rooms_number_not_empty`

### reservations

Stores guest reservations assigned to rooms.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| id | uuid | yes | Primary key |
| guest_first_name | varchar(80) | yes | Guest first name |
| guest_last_name | varchar(80) | yes | Guest last name |
| email | varchar(160) | yes | Guest email |
| check_in | date | yes | Reservation start date |
| check_out | date | yes | Reservation end date |
| room_id | uuid | yes | Foreign key to `rooms.id` |
| status | varchar(30) | yes | Reservation status enum |
| created_at_utc | timestamptz | yes | Creation timestamp |
| updated_at_utc | timestamptz | no | Last update timestamp |

Relationships:

- Many reservations belong to one room.
- Deleting a room is restricted while reservations reference it.

Indexes and constraints:

- `pk_reservations`
- `fk_reservations_rooms_room_id`
- `ix_reservations_room_dates`
- `ix_reservations_email`
- `ck_reservations_check_out_after_check_in`
- `ck_reservations_guest_first_name_not_empty`
- `ck_reservations_guest_last_name_not_empty`
- `ck_reservations_email_not_empty`

### vehicles

Stores guest vehicles linked to reservations for parking access.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| id | uuid | yes | Primary key |
| registration_number | varchar(15) | yes | Unique normalized plate number |
| reservation_id | uuid | yes | Foreign key to `reservations.id` |
| parking_access_from | timestamptz | yes | Parking access start |
| parking_access_to | timestamptz | yes | Parking access end |
| is_active | boolean | yes | Whether access is active |

Relationships:

- Many vehicles can belong to one reservation.
- Vehicles are deleted when the reservation is deleted.

Indexes and constraints:

- `pk_vehicles`
- `fk_vehicles_reservations_reservation_id`
- `ux_vehicles_registration_number`
- `ix_vehicles_reservation_active`
- `ck_vehicles_registration_number_not_empty`
- `ck_vehicles_access_to_after_access_from`

### users

Stores PMS employee accounts and access roles.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| id | uuid | yes | Primary key |
| username | varchar(80) | yes | Unique username |
| email | varchar(160) | yes | Unique email |
| password_hash | varchar(500) | yes | Hashed password |
| role | varchar(30) | yes | User role enum |

Indexes and constraints:

- `pk_users`
- `ux_users_username`
- `ux_users_email`
- `ck_users_username_not_empty`
- `ck_users_email_not_empty`
- `ck_users_password_hash_not_empty`

### parking_logs

Stores parking gate verification events. This table fulfills the MVP logging requirement for parking access events.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| id | uuid | yes | Primary key |
| vehicle_id | uuid | no | Optional foreign key to `vehicles.id` |
| registration_number | varchar(15) | yes | Plate detected or entered at the gate |
| timestamp | timestamptz | yes | Event time |
| event_type | varchar(20) | yes | Entry or Exit |
| verification_status | varchar(30) | yes | Allowed, denied, unknown, etc. |

Relationships:

- A parking log can reference one vehicle.
- If a vehicle is deleted, the log remains and `vehicle_id` is set to null.

Indexes and constraints:

- `pk_parking_logs`
- `fk_parking_logs_vehicles_vehicle_id`
- `ix_parking_logs_registration_timestamp`
- `ix_parking_logs_vehicle_id`
- `ck_parking_logs_registration_number_not_empty`

## Entity Framework Core

DbContext:

```text
src/HotelManagement.Infrastructure/Persistence/ApplicationDbContext.cs
```

Entity configurations:

```text
src/HotelManagement.Infrastructure/Persistence/Configurations/
├── RoomConfiguration.cs
├── ReservationConfiguration.cs
├── VehicleConfiguration.cs
├── UserConfiguration.cs
└── ParkingLogConfiguration.cs
```

Migrations:

```text
src/HotelManagement.Infrastructure/Persistence/Migrations/
```

## Migration commands

Apply migrations locally:

```powershell
dotnet ef database update --project .\src\HotelManagement.Infrastructure --startup-project .\src\HotelManagement.Api
```

Create a migration:

```powershell
dotnet ef migrations add MigrationName --project .\src\HotelManagement.Infrastructure --startup-project .\src\HotelManagement.Api --output-dir Persistence\Migrations
```

## Completion criteria mapping

- PostgreSQL schema is designed through EF Core code-first configuration.
- Core MVP tables are defined: `rooms`, `reservations`, `vehicles`, `users`, `parking_logs`.
- Primary keys and foreign keys are configured.
- Room-reservation, reservation-vehicle, and vehicle-log relationships are configured.
- Basic data integrity constraints are configured.
- Initial migration exists and the database can be created by applying migrations locally.
