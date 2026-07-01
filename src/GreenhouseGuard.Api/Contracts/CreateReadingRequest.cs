using System.ComponentModel.DataAnnotations;

namespace GreenhouseGuard.Api.Contracts;

public sealed record CreateReadingRequest(
    [Range(-40, 85)] decimal Temperature,
    [Range(0, 100)] decimal Humidity,
    [Range(300, 5000)] int Co2Ppm);