using Microsoft.AspNetCore.SignalR;

namespace GreenhouseGuard.Infrastructure.Hubs;

public sealed class SensorHub : Hub
{
    public const string HubPath = "/hub/sensors";

    public Task Heartbeat()
    {
        return Task.CompletedTask;
    }
}