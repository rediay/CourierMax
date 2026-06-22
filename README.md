# CourierMax

Courier logistics management API built with ASP.NET Core 9.0.

## Architecture

Clean Architecture with separate projects for Domain, Application, Infrastructure, and WebApi.

## Tech Stack

- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core 9.0 + SQL Server
- xUnit + Moq + FluentAssertions
- Swagger/OpenAPI

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (local or remote)

## Setup

1. Clone the repository
2. Create the database using `database/init.sql`
3. Update the connection string in `src/CourierMax.WebApi/appsettings.json`
4. Run the application:

```bash
cd CourierMax/CourierMax.WebApi
dotnet run
```

5. Open Swagger UI at `https://localhost:5001/swagger`

## Connection String

Configure in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=Yadier;Database=CourierMax;User ID=sa;Password=Yadier9411;MultipleActiveResultSets=true;TrustServerCertificate=True;"
  }
}
```

## Run Tests

```bash
dotnet test
```

(Full documentation and API examples coming soon)
