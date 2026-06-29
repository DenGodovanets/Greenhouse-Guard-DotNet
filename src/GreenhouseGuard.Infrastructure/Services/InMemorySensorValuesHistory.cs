using GreenhouseGuard.Application.Abstractions.Services;
using GreenhouseGuard.Application.Configuration;
using Microsoft.Extensions.Options;

namespace GreenhouseGuard.Infrastructure.Services;

public sealed class InMemorySensorValuesHistory : ISensorValuesHistory
{
    private readonly Dictionary<string, Queue<decimal>> _sensorWindows = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _syncRoot = new();
    private readonly int _windowSize;

    public InMemorySensorValuesHistory(IOptions<SensorMonitoringOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _windowSize = options.Value.WindowSize > 0 ? options.Value.WindowSize : 20;
    }

    public void AddValue(string sensorType, decimal value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sensorType);

        lock (_syncRoot)
        {
            if (!_sensorWindows.TryGetValue(sensorType, out var window))
            {
                window = new Queue<decimal>(_windowSize);
                _sensorWindows[sensorType] = window;
            }

            if (window.Count == _windowSize) window.Dequeue();

            window.Enqueue(value);
        }
    }

    public IReadOnlyCollection<decimal> GetValues(string sensorType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sensorType);

        lock (_syncRoot)
        {
            if (!_sensorWindows.TryGetValue(sensorType, out var window)) return Array.Empty<decimal>();

            return window.ToArray();
        }
    }
}