using GreenhouseGuard.Domain.Common;

namespace GreenhouseGuard.Domain.Entities;

public class Anomaly : Entity
{
    private Anomaly()
    {
    }

    public DateTime DetectedAt { get; private set; }
    public string SensorType { get; private set; } = string.Empty;
    public decimal Value { get; private set; }
    public decimal ZScore { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    public static Anomaly Create(
        string sensorType,
        decimal value,
        decimal zScore,
        string reason)
    {
        return new Anomaly
        {
            Id = Guid.NewGuid(),
            DetectedAt = DateTime.UtcNow,
            SensorType = sensorType,
            Value = value,
            ZScore = zScore,
            Reason = reason
        };
    }
}