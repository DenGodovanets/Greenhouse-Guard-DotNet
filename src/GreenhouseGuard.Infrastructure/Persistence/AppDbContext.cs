using GreenhouseGuard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseGuard.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<Anomaly> Anomalies => Set<Anomaly>();
}