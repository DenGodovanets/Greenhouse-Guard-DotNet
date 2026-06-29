using GreenhouseGuard.Application.DTOs;

namespace GreenhouseGuard.Application.Abstractions.Hubs;

public interface ISensorHub
{
    Task NotifyNewReadingAsync(SensorReadingDto reading, CancellationToken cancellationToken = default);

    Task NotifyAnomalyAsync(AnomalyDto anomaly, CancellationToken cancellationToken = default);
}