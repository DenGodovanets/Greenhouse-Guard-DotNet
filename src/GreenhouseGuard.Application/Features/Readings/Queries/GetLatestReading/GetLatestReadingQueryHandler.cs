using GreenhouseGuard.Application.Abstractions.Repositories;
using GreenhouseGuard.Application.DTOs;
using MediatR;

namespace GreenhouseGuard.Application.Features.Readings.Queries.GetLatestReading;

internal sealed class GetLatestReadingQueryHandler : IRequestHandler<GetLatestReadingQuery, SensorReadingDto?>
{
    private readonly IReadingRepository _readingRepository;

    public GetLatestReadingQueryHandler(IReadingRepository readingRepository)
    {
        _readingRepository = readingRepository;
    }

    public async Task<SensorReadingDto?> Handle(GetLatestReadingQuery request, CancellationToken cancellationToken)
    {
        var reading = await _readingRepository.GetLatestAsync(cancellationToken);

        if (reading is null) return null;

        return new SensorReadingDto(
            reading.Id,
            reading.SequenceNumber,
            reading.Timestamp,
            reading.Temperature,
            reading.Humidity,
            reading.Co2Ppm);
    }
}