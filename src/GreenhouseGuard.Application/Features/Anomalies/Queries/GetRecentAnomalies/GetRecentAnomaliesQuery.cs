using GreenhouseGuard.Application.DTOs;
using MediatR;

namespace GreenhouseGuard.Application.Features.Anomalies.Queries.GetRecentAnomalies;

public sealed record GetRecentAnomaliesQuery : IRequest<IReadOnlyList<AnomalyDto>>;