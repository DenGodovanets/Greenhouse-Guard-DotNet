namespace GreenhouseGuard.Application.DTOs;

public sealed record AnomalyDto(
    Guid Id,
    DateTime DetectedAt,
    string SensorType,
    decimal Value,
    decimal ZScore,
    string Reason);