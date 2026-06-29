using GreenhouseGuard.Domain.Common;

namespace GreenhouseGuard.Domain.Entities;

public class SensorReading : Entity
{
    private SensorReading()
    {
    }

    public long SequenceNumber { get; private set; }
    public DateTime Timestamp { get; private set; }
    public decimal Temperature { get; private set; }
    public decimal Humidity { get; private set; }
    public int Co2Ppm { get; private set; }

    public static SensorReading Create(
        decimal temperature,
        decimal humidity,
        int co2Ppm,
        long sequenceNumber)
    {
        return new SensorReading
        {
            Id = Guid.NewGuid(),
            SequenceNumber = sequenceNumber,
            Timestamp = DateTime.UtcNow,
            Temperature = temperature,
            Humidity = humidity,
            Co2Ppm = co2Ppm
        };
    }
}