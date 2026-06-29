using GreenhouseGuard.Application.Abstractions.Repositories;
using GreenhouseGuard.Domain.Entities;
using GreenhouseGuard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseGuard.Infrastructure.Repositories;

public sealed class AnomalyRepository : IAnomalyRepository
{
    private readonly AppDbContext _dbContext;

    public AnomalyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Anomaly>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Anomalies
            .AsNoTracking()
            .OrderByDescending(x => x.DetectedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Anomaly anomaly, CancellationToken cancellationToken = default)
    {
        await _dbContext.Anomalies.AddAsync(anomaly, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}