using System.ComponentModel.DataAnnotations;

namespace GreenhouseGuard.Api.Contracts;

public sealed record CreateReadingRequest(
    [property: Range(-40, 85)] decimal Temperature,
    [property: Range(0, 100)] decimal Humidity,
    [property: Range(300, 5000)] int Co2Ppm);