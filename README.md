# IBTS2026 - Incident Tracking System

A modern incident management system built with .NET Aspire, Blazor Server, and Entity Framework Core.

## Architecture

- **IBTS2026.AppHost** - .NET Aspire orchestration
- **IBTS2026.Web** - Blazor Server frontend
- **IBTS2026.ApiService** - REST API backend
- **IBTS2026.Worker** - Background job processor
- **IBTS2026.Application** - Business logic (CQRS handlers)
- **IBTS2026.Infrastructure** - Data access, repositories, services
- **IBTS2026.Domain** - Entities and domain logic

## Features

- **User Management** - Invitation-based registration, role-based access (Admin/User)
- **Incident Tracking** - Create, update, assign incidents with status/priority
- **Incident Notes** - Journal entries on incidents
- **Notifications** - Email notifications via outbox pattern for assignments, status changes, notes
- **Security** - JWT authentication with SecurityStamp invalidation

## Prerequisites

- .NET 10 SDK
- Docker Desktop (for SQL Server and Redis containers)
- Visual Studio 2022 or VS Code

## Getting Started

1. Clone the repository
2. Run the AppHost project:
   ```bash
   cd IBTS2026/IBTS2026.AppHost
   dotnet run
   ```
3. Aspire will start SQL Server, Redis, API, Web, and Worker services
4. Open the Aspire dashboard URL shown in console
5. Navigate to the Web frontend URL
6. First user to register becomes Admin

## Development SMTP

For email testing, install Papercut SMTP:
- Download from GitHub releases https://github.com/ChangemakerStudios/Papercut-SMTP
- Runs on port 2525 (configured in appsettings)
- Web UI at http://localhost:8080

## Configuration

Key settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-secret-key-min-32-chars",
    "ExpirationMinutes": 60
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "2525",
    "FromAddress": "noreply@ibts2026.local"
  }
}
```

## Database

- SQL Server with EF Core migrations
- Temporal tables for audit history
- Migrations auto-apply on startup

## API Endpoints

- `POST /auth/register` - Register new user
- `POST /auth/login` - Login, returns JWT
- `POST /auth/invite` - Invite user (Admin)
- `GET /api/incidents` - List incidents
- `POST /api/incidents` - Create incident
- `PUT /api/incidents/{id}` - Update incident
- `GET /api/users` - List users

## License

Proprietary
