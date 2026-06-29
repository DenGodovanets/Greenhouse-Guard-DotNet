# Greenhouse Guard — Backend API

Real-time greenhouse monitoring system with anomaly detection, sensor data management, and live WebSocket updates.

**Status:** Production-ready | **Framework:** ASP.NET Core 9 | **License:** MIT

---

## Features

- ✅ **Real-time Sensor Monitoring** - Temperature, humidity, CO2 tracking
- ✅ **Anomaly Detection** - Z-score statistical analysis for outliers
- ✅ **Live Updates** - SignalR WebSocket for instant alerts
- ✅ **Persistent Storage** - Entity Framework Core with in-memory database
- ✅ **CQRS Pattern** - Clean separation via MediatR
- ✅ **Comprehensive Tests** - 14+ unit tests with 100% pass rate
- ✅ **API Documentation** - Scalar OpenAPI reference UI

---

## Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **API** | ASP.NET Core 9 | REST endpoints, WebSocket hub |
| **Architecture** | MediatR | CQRS command/query separation |
| **Data** | Entity Framework Core | Database abstraction (in-memory) |
| **Real-time** | SignalR | Bi-directional WebSocket communication |
| **Testing** | xUnit, FluentAssertions | Unit test framework |
| **API Docs** | Scalar | Interactive OpenAPI documentation |

---

## Project Structure

```
src/
├── GreenhouseGuard.Api/
│   ├── Program.cs                    # DI setup, middleware configuration
│   ├── Controllers/
│   │   ├── ReadingsController.cs     # POST /readings (sensor data ingestion)
│   │   └── AnomaliesController.cs    # GET /anomalies (anomaly history)
│   ├── Contracts/
│   │   └── CreateReadingRequest.cs   # API request DTO
│   └── appsettings.json              # Configuration (CORS, thresholds)
│
├── GreenhouseGuard.Application/
│   ├── Configuration/
│   │   └── SensorMonitoringOptions.cs # WindowSize=20, ZScoreThreshold=2.5
│   ├── Features/
│   │   └── Readings/
│   │       └── Commands/
│   │           └── CreateReading/    # MediatR command handler
│   ├── Abstractions/
│   │   ├── Repositories/             # IReadingRepository, IAnomalyRepository
│   │   ├── Services/                 # ISensorValuesHistory, ISensorHub
│   │   └── Hubs/                     # SignalR abstractions
│   └── DependencyInjection.cs        # Application layer registration
│
├── GreenhouseGuard.Domain/
│   ├── Entities/
│   │   ├── SensorReading.cs          # Temperature, Humidity, CO2 values
│   │   └── Anomaly.cs                # Detected anomalies with Z-score
│   ├── Services/
│   │   └── AnomalyDetector.cs        # Z-score calculation logic
│   └── Common/
│       └── Entity.cs                 # Base entity with Id
│
├── GreenhouseGuard.Infrastructure/
│   ├── BackgroundServices/
│   │   └── SensorDataSimulator.cs    # Runs every 3s, generates test data
│   ├── Hubs/
│   │   └── SensorHubNotifier.cs      # Broadcasts anomalies via SignalR
│   ├── Persistence/
│   │   └── AppDbContext.cs           # EF Core DbContext
│   ├── Repositories/
│   │   ├── ReadingRepository.cs      # CRUD for sensor readings
│   │   └── AnomalyRepository.cs      # CRUD for anomalies
│   ├── Services/
│   │   └── InMemorySensorValuesHistory.cs # Sliding window buffer
│   └── DependencyInjection.cs        # Infrastructure layer registration
│
tests/
├── GreenhouseGuard.Tests/
│   ├── AnomalyDetectorTests.cs       # 4 tests: spike, normal, edge cases
│   ├── InMemorySensorValuesHistoryTests.cs # 10 tests: window, concurrency
│   ├── README.md                     # Test documentation
│   └── GreenhouseGuard.Tests.csproj
```

---

## Quick Start

### Prerequisites

- [.NET SDK 9+](https://dotnet.microsoft.com/download)
- macOS / Linux / Windows

### Installation

```bash
# Clone repository
git clone <repo-url>
cd Greenhouse-Guard-DotNet

# Restore dependencies
dotnet restore

# Run API
dotnet run --project src/GreenhouseGuard.Api
```

**API Endpoints:**
- REST: `http://localhost:5233`
- Scalar Docs: `http://localhost:5233/scalar/v1`
- SignalR Hub: `ws://localhost:5233/hubs/sensor`

---

## API Endpoints

### Create Sensor Reading
```http
POST /api/readings
Content-Type: application/json

{
  "temperature": 22.5,
  "humidity": 65.0,
  "co2Ppm": 810
}
```

**Response:** 
```json
{
  "id": "uuid",
  "temperature": 22.5,
  "humidity": 65.0,
  "co2Ppm": 810,
  "recordedAt": "2026-06-29T20:45:00Z"
}
```

### Get Recent Anomalies
```http
GET /api/anomalies?count=10
```

**Response:**
```json
[
  {
    "id": "uuid",
    "sensorType": "Temperature",
    "value": 35.8,
    "zScore": 12.34,
    "reason": "Z-score 12.34 exceeded threshold 2.50 for Temperature.",
    "detectedAt": "2026-06-29T20:45:15Z"
  }
]
```

---

## Architecture Pattern: CQRS

**Command:** Mutates state (POST/PUT/DELETE)
```
POST /readings
  ↓
CreateReadingCommand
  ↓
CreateReadingCommandHandler
  ├→ AddValue() to history
  ├→ Detect() anomalies
  ├→ Persist to database
  └→ Broadcast via SignalR
```

**Query:** Reads state (GET)
```
GET /anomalies
  ↓
Database Query via Repository
  ↓
Response DTO
```

---

## Anomaly Detection Flow

### Z-Score Formula
```
Z = |value - mean| / standard_deviation
Anomaly detected if: |Z| > 2.5
```

### Real Example
```
Historical temps: [20.0, 21.0, 22.0, 21.5, 20.5]°C
  Mean = 21.0°C
  StdDev ≈ 0.87°C

New reading: 35.0°C
  Z-score = |35.0 - 21.0| / 0.87 = 16.1
  Result: ANOMALY (Z > 2.5)
  
Action:
  → Persist to database
  → Broadcast to connected clients via SignalR
```

### Sliding Window (Last 20 Readings)
- Maintains FIFO queue per sensor type
- Thread-safe with lock synchronization
- Automatically discards oldest value on overflow
- Configuration: `SensorMonitoring.WindowSize=20`

---

## Configuration

**File:** `src/GreenhouseGuard.Api/appsettings.json`

```json
{
  "SensorMonitoring": {
    "WindowSize": 20,              // Historical buffer size
    "ZScoreThreshold": 2.5         // Anomaly detection threshold
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",     // Angular dev server
      "http://localhost:3000"      // React/Vue dev
    ]
  }
}
```

**Environment-specific:**
- `appsettings.Development.json` - Local development
- `appsettings.json` - Production defaults

---

## Running Tests

### All Tests
```bash
dotnet test tests/GreenhouseGuard.Tests/
```

### Specific Test Class
```bash
dotnet test tests/GreenhouseGuard.Tests/ --filter "AnomalyDetectorTests"
```

### With Verbosity
```bash
dotnet test tests/GreenhouseGuard.Tests/ --verbosity normal
```

**Test Coverage:**
| Component | Tests |
|-----------|-------|
| AnomalyDetector | 4 |
| InMemorySensorValuesHistory | 10 |
| **Total** | **14** |

See [tests/GreenhouseGuard.Tests/README.md](tests/GreenhouseGuard.Tests/README.md) for detailed test documentation.

---

## Key Services

### AnomalyDetector (Domain Service)
**Purpose:** Detects statistical anomalies using Z-score method

```csharp
var anomaly = detector.Detect(
    sensorType: "Temperature",
    value: 35.8m,
    historicalValues: new[] { 22m, 22m, 22m, 22m, 22m },
    requiredSampleSize: 3,
    zScoreThreshold: 2.5m
);
// Returns: Anomaly or null
```

**Edge Cases Handled:**
- Insufficient historical data (< 3 readings)
- Zero standard deviation (identical values)
- Division by zero protection

### InMemorySensorValuesHistory (Infrastructure Service)
**Purpose:** Maintains sliding window of sensor readings per type

```csharp
history.AddValue("Temperature", 22.5m);      // Add reading
history.GetValues("Temperature");             // Get last 20 readings
```

**Features:**
- FIFO queue (First In, First Out)
- Case-insensitive sensor type keys
- Thread-safe (lock-based synchronization)
- Automatic overflow handling

### CreateReadingCommandHandler (Application Handler)
**Purpose:** Processes incoming sensor readings and coordinates anomaly detection

**Flow:**
1. Validate reading data
2. Add value to history
3. Run anomaly detection
4. Persist reading + any anomalies
5. Broadcast anomalies via SignalR

---

## Dependency Injection

**API Layer:**
```csharp
builder.Services.AddApplication();      // Controllers, MediatR
builder.Services.AddInfrastructure();    // Repositories, services
```

**Pattern:** Constructor injection with `IServiceProvider`

---

## Background Service: SensorDataSimulator

**Purpose:** Generates test sensor data every 3 seconds (development/testing)

**Behavior:**
- Runs continuously after API startup
- Simulates realistic greenhouse fluctuations
- Creates `CreateReadingCommand` via MediatR
- Stops gracefully on application shutdown

**Data Range:**
- Temperature: 15–30°C with random variance
- Humidity: 40–80%
- CO2: 400–1200 ppm

**Disable in Production:**
```csharp
// Program.cs
if (app.Environment.IsProduction())
{
    // services.AddHostedService<SensorDataSimulator>();
}
```

---

## SignalR Hub: Real-Time Anomaly Broadcasts

**Client Connection:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5233/hubs/sensor")
    .withAutomaticReconnect()
    .build();

connection.on("ReceiveAnomaly", (anomaly) => {
    console.log("Anomaly detected:", anomaly);
});

await connection.start();
```

**Server Broadcast:**
```csharp
await _sensorHub.NotifyAnomalyAsync(anomalyDto, cancellationToken);
```

---

## Best Practices Applied

### 1. Clean Architecture
- **Domain:** Business logic, no dependencies
- **Application:** Use cases, CQRS commands/queries
- **Infrastructure:** Database, external services
- **API:** Controllers, HTTP concerns only

### 2. CQRS Pattern (MediatR)
- **Commands:** `IRequest<TResponse>` + `IRequestHandler<,>`
- **Queries:** `IRequest<TResponse>` + `IRequestHandler<,>`
- **Single responsibility:** Each command/query has one handler

### 3. Dependency Injection
- Constructor injection preferred
- `IServiceProvider` for factories
- Lifecycle: Scoped (per-request), Singleton, Transient

### 4. Configuration Management
- `IOptions<T>` pattern for type-safe config
- Environment-specific appsettings
- Strongly-typed `SensorMonitoringOptions`

### 5. Thread Safety
- Lock-based synchronization for `InMemorySensorValuesHistory`
- Immutable DTOs for data transfer
- No shared mutable state between services

### 6. Testing
- Unit tests for domain logic and services
- Arrange-Act-Assert pattern
- FluentAssertions for readability
- Edge cases and boundary testing

### 7. Error Handling
- `ArgumentNullException` / `ArgumentException` for invalid input
- Try-catch in background service (graceful shutdown)
- Logging via `ILogger<T>`

---

## Common Tasks

### Add a New Sensor Type

1. **Domain:** Add property to `SensorReading.cs`
2. **Application:** Update `CreateReadingCommand` and handler
3. **API:** Update `CreateReadingRequest` DTO
4. **Tests:** Add test cases for anomaly detection

### Change Anomaly Threshold

Edit `appsettings.json`:
```json
"SensorMonitoring": {
  "ZScoreThreshold": 3.0  // More lenient (was 2.5)
}
```

Restart API for changes to take effect.

### Add New Repository

1. Create interface in `Application/Abstractions/Repositories/`
2. Create implementation in `Infrastructure/Repositories/`
3. Register in `Infrastructure/DependencyInjection.cs`
4. Inject via constructor in handlers

---

## Performance Considerations

| Operation | Complexity | Max Load |
|-----------|-----------|----------|
| AddValue (history) | O(1) | 1000s/sec |
| Detect (anomaly) | O(n) where n=20 | <1ms |
| Get history | O(1) | Immediate |
| Concurrent writes | Thread-safe | 100+ threads |

**Bottleneck:** Database saves (`SaveChangesAsync`). Consider:
- Batch writes for high-volume ingestion
- Read replicas for GET operations
- Redis cache for historical queries

---

## Troubleshooting

### Tests Fail: "Insufficient History"
**Solution:** Tests require ≥ 3 historical readings before anomaly detection can trigger.

### API Won't Start: Port Already in Use
**Solution:** Change `appsettings.json`:
```json
"Kestrel": {
  "Endpoints": {
    "Http": { "Url": "http://localhost:5234" }
  }
}
```

### CORS Error: No 'Access-Control-Allow-Origin'
**Solution:** Add frontend URL to `appsettings.json`:
```json
"Cors": {
  "AllowedOrigins": ["http://localhost:4200"]
}
```

---

## Development Workflow

```bash
# 1. Restore & Build
dotnet restore && dotnet build

# 2. Run Tests
dotnet test

# 3. Start API
dotnet run --project src/GreenhouseGuard.Api

# 4. Test Endpoint (curl)
curl -X POST http://localhost:5233/api/readings \
  -H "Content-Type: application/json" \
  -d '{"temperature": 22.5, "humidity": 65.0, "co2Ppm": 810}'

# 5. View API Docs
open http://localhost:5233/scalar/v1
```

---

## Contributing

### Code Standards
- Use `var` when type is obvious; explicit type for unclear cases
- Null-coalescing operators: `??` and `??=`
- async/await for I/O operations
- LINQ for collections, not loops

### Naming Convention
```csharp
Services:       IReadingRepository, SensorDataSimulator
Commands:       CreateReadingCommand, UpdateAnomalyStatusCommand
Handlers:       CreateReadingCommandHandler
DTOs:           CreateReadingRequest, SensorReadingDto
```

### Before Commit
```bash
dotnet format                    # Auto-format code
dotnet build                     # Build
dotnet test                      # Run tests
```

---

## References

- [ASP.NET Core Docs](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [Z-Score Wikipedia](https://en.wikipedia.org/wiki/Standard_score)

---

## License

MIT License — See LICENSE file for details

---

**Last Updated:** June 29, 2026  
**Maintainers:** Denys Godovanets

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
