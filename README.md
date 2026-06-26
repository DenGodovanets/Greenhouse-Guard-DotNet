# Greenhouse Guard — .NET Backend

REST API and backend services for the Greenhouse Guard monitoring system.

## Tech Stack

- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) (.NET 9)
- Entity Framework Core (In-Memory)
- SignalR _(real-time sensor updates)_
- MediatR (CQRS)
- Scalar (API reference UI)

## Prerequisites

- [.NET SDK 9+](https://dotnet.microsoft.com/download)

## Getting Started

```bash
# Restore dependencies
dotnet restore

# Run the API
dotnet run --project src/GreenhouseGuard.Api
```

API will be available at `https://localhost:7251` / `http://localhost:5233`.

API Reference (Scalar): `http://localhost:5233/scalar/v1`

## Available Commands

| Command | Description |
|---------|-------------|
| `dotnet restore` | Restore NuGet packages |
| `dotnet build` | Build the solution |
| `dotnet test` | Run all unit tests |
| `dotnet run --project src/GreenhouseGuard.Api` | Start the API locally |

## Project Structure

```
src/
├── GreenhouseGuard.Api/          # ASP.NET Core Web API (entry point)
├── GreenhouseGuard.Application/  # Business logic, CQRS handlers
├── GreenhouseGuard.Domain/       # Domain entities, interfaces
└── GreenhouseGuard.Infrastructure/ # EF Core, external services
tests/
└── GreenhouseGuard.Tests/
```

## Configuration

The app uses **EF Core In-Memory** database by default — no external database required.

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'feat: add your feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

## License

MIT
