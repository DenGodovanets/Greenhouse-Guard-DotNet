namespace GreenhouseGuard.Application.DTOs;

public sealed record SensorReadingDto(
    Guid Id,
    long SequenceNumber,
    DateTime Timestamp,
    decimal Temperature,
    decimal Humidity,
    int Co2Ppm);