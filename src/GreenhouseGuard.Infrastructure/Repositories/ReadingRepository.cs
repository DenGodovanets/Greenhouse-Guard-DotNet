using GreenhouseGuard.Application.Abstractions.Repositories;
using GreenhouseGuard.Domain.Entities;
using GreenhouseGuard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenhouseGuard.Infrastructure.Repositories;

public sealed class ReadingRepository : IReadingRepository
{
    private readonly AppDbContext _dbContext;

    public ReadingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SensorReading?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SensorReadings
            .AsNoTracking()
            .OrderByDescending(x => x.SequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<long> GetNextSequenceNumberAsync(CancellationToken cancellationToken = default)
    {
        var maxSequence = await _dbContext.SensorReadings
            .Select(x => (long?)x.SequenceNumber)
            .MaxAsync(cancellationToken);

        return (maxSequence ?? 0) + 1;
    }

    public async Task AddAsync(SensorReading reading, CancellationToken cancellationToken = default)
    {
        await _dbContext.SensorReadings.AddAsync(reading, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}