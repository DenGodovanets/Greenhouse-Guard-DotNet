using GreenhouseGuard.Domain.Entities;

namespace GreenhouseGuard.Application.Abstractions.Repositories;

public interface IReadingRepository
{
    Task<SensorReading?> GetLatestAsync(CancellationToken cancellationToken = default);

    Task<long> GetNextSequenceNumberAsync(CancellationToken cancellationToken = default);

    Task AddAsync(SensorReading reading, CancellationToken cancellationToken = default);
}