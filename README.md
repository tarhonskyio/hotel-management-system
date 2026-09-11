# Hotel Management System

Modular PMS application for small and medium-sized hotels. The current MVP contains an ASP.NET Core Web API backend, PostgreSQL persistence with Entity Framework Core, and a React + Vite frontend.

## Repository structure

```text
hotel-management-system/
├── src/
│   ├── HotelManagement.Api/
│   ├── HotelManagement.Application/
│   ├── HotelManagement.Domain/
│   └── HotelManagement.Infrastructure/
├── tests/
│   └── HotelManagement.Tests/
├── frontend/
├── docs/
├── docker-compose.yml
└── README.md
```

## Backend stack

- .NET 8 ASP.NET Core Web API
- PostgreSQL
- Entity Framework Core, code-first migrations
- JWT bearer authentication
- Swagger/OpenAPI in Development
- Layered architecture: Api, Application, Domain, Infrastructure

## Local setup

1. Copy environment example for PostgreSQL:

   ```powershell
   Copy-Item .\.env.example .\.env
   ```

2. Start PostgreSQL with Docker:

   ```powershell
   docker compose up -d postgres
   ```

3. Create local backend configuration:

   ```powershell
   Copy-Item .\src\HotelManagement.Api\appsettings.Local.example.json .\src\HotelManagement.Api\appsettings.Local.json
   ```

   If you changed `POSTGRES_PASSWORD` in `.env`, update the same password in `appsettings.Local.json`.

4. Apply EF Core migrations:

   ```powershell
   dotnet ef database update --project .\src\HotelManagement.Infrastructure --startup-project .\src\HotelManagement.Api
   ```

5. Run the backend:

   ```powershell
   dotnet run --project .\src\HotelManagement.Api\HotelManagement.Api.csproj
   ```

   API: [http://localhost:5114](http://localhost:5114)

   Swagger: [http://localhost:5114/swagger](http://localhost:5114/swagger)

6. Run the frontend:

   ```powershell
   cd frontend
   npm install
   npm run dev
   ```

   Frontend: [http://localhost:5173](http://localhost:5173)

## Verification

```powershell
dotnet restore
dotnet build .\HotelManagementSystem.sln
dotnet test .\HotelManagementSystem.sln

cd frontend
npm run build
npm run lint
```

## Secrets

Do not commit real passwords or local secrets. Use:

- root `.env` for Docker-only local variables;
- `src/HotelManagement.Api/appsettings.Local.json` for local backend connection strings.

Both are ignored by git.
