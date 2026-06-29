using GreenhouseGuard.Application.DTOs;
using MediatR;

namespace GreenhouseGuard.Application.Features.Readings.Commands.CreateReading;

public sealed record CreateReadingCommand(
    decimal Temperature,
    decimal Humidity,
    int Co2Ppm) : IRequest<SensorReadingDto>;