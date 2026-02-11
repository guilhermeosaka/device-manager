# Device Manager

A .NET-based service API with PostgreSQL database support.

## Prerequisites

- [Docker](https://www.docker.com/get-started)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Optional) Entity Framework Core CLI tools

## Running locally

Run the following commands inside the root folder.

### 1. Start dependencies (PostgreSQL database)

Note: This will also start the containerized application on port 5000

```bash
docker compose up -d
```

### 2. Run the application locally

```bash
dotnet run --project src\DeviceManager.Api
```

or via IDE of your choice using `DeviceManager.Api` as the startup project.

Note: `launchSettings.json` is configured to run the application on port `4000`

### 3. Access the Swagger UI

Swagger URL: http://localhost:4000/swagger

## Database Migration

### 1. Make sure you have EF Core CLI tools installed.

```bash
dotnet tool install --global dotnet-ef
```

### 2. Create Migration
```bash
dotnet ef migrations add <name> --project .\src\DeviceManager.Infrastructure --startup-project .\src\DeviceManager.Api
```

### 3. (Optional) Update Database Schema

Note: Migration runs at application startup

```bash
dotnet ef database update --project .\src\DeviceManager.Infrastructure --startup-project .\src\DeviceManager.Api
```

## Testing Dockerfile

To test if the Dockerfile builds correctly:

```bash
docker build -f .\src\DeviceManager.Api\Dockerfile -t device-manager-api .
```