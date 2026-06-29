using GreenhouseGuard.Application.DTOs;
using MediatR;

namespace GreenhouseGuard.Application.Features.Readings.Queries.GetLatestReading;

public sealed record GetLatestReadingQuery : IRequest<SensorReadingDto?>;