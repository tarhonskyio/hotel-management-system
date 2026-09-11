# Task 1.1 — Project setup

This document describes the initial technical setup for the Hotel Management System.

## .NET 8 Web API

The backend is configured as an ASP.NET Core Web API targeting `.NET 8`.

Main API project:

```text
src/HotelManagement.Api/HotelManagement.Api.csproj
```

The API project enables:

- nullable reference types;
- implicit usings;
- XML documentation generation for Swagger/OpenAPI;
- JWT bearer authentication;
- controllers;
- Swagger UI in Development.

Local API URL:

```text
http://localhost:5114
```

Swagger URL:

```text
http://localhost:5114/swagger
```

## Layered project structure

The backend is split into four projects:

```text
src/
├── HotelManagement.Api
├── HotelManagement.Application
├── HotelManagement.Domain
└── HotelManagement.Infrastructure
```

Responsibilities:

- `HotelManagement.Api` — HTTP controllers, middleware, authentication and Swagger configuration.
- `HotelManagement.Application` — DTOs, validators, interfaces and business services.
- `HotelManagement.Domain` — entities and enums.
- `HotelManagement.Infrastructure` — Entity Framework Core DbContext, entity configurations, migrations and repository implementation.

## PostgreSQL configuration

PostgreSQL is configured through the `PostgreSql` connection string:

```json
{
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Port=5432;Database=hotel_pms;Username=postgres;Password=YOUR_LOCAL_POSTGRES_PASSWORD"
  }
}
```

For local development, put the real password in:

```text
src/HotelManagement.Api/appsettings.Local.json
```

This file is ignored by git.

The committed example file is:

```text
src/HotelManagement.Api/appsettings.Local.example.json
```

## Docker PostgreSQL

The repository includes `docker-compose.yml` with a PostgreSQL 17 service.

Start the database:

```powershell
docker compose up -d postgres
```

Default local values are defined in `.env.example`:

```text
POSTGRES_DB=hotel_pms
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5432
```

Copy it before first use:

```powershell
Copy-Item .\.env.example .\.env
```

## Entity Framework Core

EF Core packages are installed in the Infrastructure project:

- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore.Design`

The DbContext is:

```text
src/HotelManagement.Infrastructure/Persistence/ApplicationDbContext.cs
```

The Infrastructure layer registers PostgreSQL with:

```csharp
options.UseNpgsql(connectionString);
```

Apply migrations:

```powershell
dotnet ef database update --project .\src\HotelManagement.Infrastructure --startup-project .\src\HotelManagement.Api
```

Create a new migration:

```powershell
dotnet ef migrations add MigrationName --project .\src\HotelManagement.Infrastructure --startup-project .\src\HotelManagement.Api --output-dir Persistence\Migrations
```

## Current MVP modules prepared by the setup

- Room management
- Reservation management
- Vehicle / parking access management
- Parking access verification
- User entity and roles
- Swagger/OpenAPI documentation
- Automated parking logic tests
