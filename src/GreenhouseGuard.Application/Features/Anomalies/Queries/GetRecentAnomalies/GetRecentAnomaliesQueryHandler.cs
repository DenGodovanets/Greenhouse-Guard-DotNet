using GreenhouseGuard.Application.Abstractions.Repositories;
using GreenhouseGuard.Application.DTOs;
using MediatR;

namespace GreenhouseGuard.Application.Features.Anomalies.Queries.GetRecentAnomalies;

internal sealed class
    GetRecentAnomaliesQueryHandler : IRequestHandler<GetRecentAnomaliesQuery, IReadOnlyList<AnomalyDto>>
{
    private const int MaxAnomalies = 20;
    private readonly IAnomalyRepository _anomalyRepository;

    public GetRecentAnomaliesQueryHandler(IAnomalyRepository anomalyRepository)
    {
        _anomalyRepository = anomalyRepository;
    }

    public async Task<IReadOnlyList<AnomalyDto>> Handle(GetRecentAnomaliesQuery request,
        CancellationToken cancellationToken)
    {
        var anomalies = await _anomalyRepository.GetRecentAsync(MaxAnomalies, cancellationToken);

        return anomalies
            .Select(a => new AnomalyDto(a.Id, a.DetectedAt, a.SensorType, a.Value, a.ZScore, a.Reason))
            .ToList();
    }
}