using GreenhouseGuard.Application.Abstractions.Hubs;
using GreenhouseGuard.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace GreenhouseGuard.Infrastructure.Hubs;

public sealed class SensorHubNotifier : ISensorHub
{
    private const string ReceiveReadingMethod = "ReceiveReading";
    private const string ReceiveAnomalyMethod = "ReceiveAnomaly";

    private readonly IHubContext<SensorHub> _hubContext;

    public SensorHubNotifier(IHubContext<SensorHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyNewReadingAsync(SensorReadingDto reading, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(ReceiveReadingMethod, reading, cancellationToken);
    }

    public Task NotifyAnomalyAsync(AnomalyDto anomaly, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(ReceiveAnomalyMethod, anomaly, cancellationToken);
    }
}