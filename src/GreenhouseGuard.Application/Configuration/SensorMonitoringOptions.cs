namespace GreenhouseGuard.Application.Configuration;

public sealed class SensorMonitoringOptions
{
    public const string SectionName = "SensorMonitoring";

    public int WindowSize { get; init; } = 20;

    public decimal ZScoreThreshold { get; init; } = 2.5m;
}