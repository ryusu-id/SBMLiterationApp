# Dev Container Setup

This dev container provides a complete development environment for the PureTCO Backend application.

## What's Included

- .NET 10.0 SDK
- PostgreSQL 16 database
- VS Code extensions for C# development
- Pre-configured database connection

## Getting Started

1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
2. Install the [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) extension in VS Code
3. Open this folder in VS Code
4. Click "Reopen in Container" when prompted (or use Command Palette: `Dev Containers: Reopen in Container`)

## Database Connection

The PostgreSQL database is automatically started and configured:
- Host: `postgres`
- Port: `5432`
- Database: `puretcowebappdb`
- Username: `postgres`
- Password: `postgres`

## Running the Application

```bash
dotnet run
```

The application will be available at `http://localhost:8080`

## Running Migrations

```bash
dotnet ef database update
```

## Useful Commands

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test

# Add a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```
