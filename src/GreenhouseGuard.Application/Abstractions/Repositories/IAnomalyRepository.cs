using GreenhouseGuard.Domain.Entities;

namespace GreenhouseGuard.Application.Abstractions.Repositories;

public interface IAnomalyRepository
{
    Task<IReadOnlyList<Anomaly>> GetRecentAsync(int count, CancellationToken cancellationToken = default);

    Task AddAsync(Anomaly anomaly, CancellationToken cancellationToken = default);
}