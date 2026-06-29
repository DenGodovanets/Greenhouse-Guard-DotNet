using GreenhouseGuard.Application.Abstractions.Hubs;
using GreenhouseGuard.Application.Abstractions.Repositories;
using GreenhouseGuard.Application.Abstractions.Services;
using GreenhouseGuard.Application.Configuration;
using GreenhouseGuard.Application.DTOs;
using GreenhouseGuard.Domain.Entities;
using GreenhouseGuard.Domain.Services;
using MediatR;
using Microsoft.Extensions.Options;

namespace GreenhouseGuard.Application.Features.Readings.Commands.CreateReading;

internal sealed class CreateReadingCommandHandler : IRequestHandler<CreateReadingCommand, SensorReadingDto>
{
    private readonly AnomalyDetector _anomalyDetector;
    private readonly IAnomalyRepository _anomalyRepository;
    private readonly SensorMonitoringOptions _options;
    private readonly IReadingRepository _readingRepository;
    private readonly ISensorHub _sensorHub;
    private readonly ISensorValuesHistory _sensorValuesHistory;

    public CreateReadingCommandHandler(
        IReadingRepository readingRepository,
        IAnomalyRepository anomalyRepository,
        ISensorValuesHistory sensorValuesHistory,
        ISensorHub sensorHub,
        AnomalyDetector anomalyDetector,
        IOptions<SensorMonitoringOptions> options)
    {
        _readingRepository = readingRepository;
        _anomalyRepository = anomalyRepository;
        _sensorValuesHistory = sensorValuesHistory;
        _sensorHub = sensorHub;
        _anomalyDetector = anomalyDetector;
        _options = options.Value;
    }

    public async Task<SensorReadingDto> Handle(CreateReadingCommand request, CancellationToken cancellationToken)
    {
        var sequenceNumber = await _readingRepository.GetNextSequenceNumberAsync(cancellationToken);

        var reading = SensorReading.Create(
            request.Temperature,
            request.Humidity,
            request.Co2Ppm,
            sequenceNumber);

        await _readingRepository.AddAsync(reading, cancellationToken);

        var readingDto = ToDto(reading);

        await _sensorHub.NotifyNewReadingAsync(readingDto, cancellationToken);

        await DetectAndBroadcastAnomaliesAsync(reading, cancellationToken);

        return readingDto;
    }

    private async Task DetectAndBroadcastAnomaliesAsync(SensorReading reading, CancellationToken cancellationToken)
    {
        var sensorValues = new[]
        {
            ("Temperature", reading.Temperature),
            ("Humidity", reading.Humidity),
            ("Co2", reading.Co2Ppm)
        };

        foreach (var (sensorType, value) in sensorValues)
        {
            _sensorValuesHistory.AddValue(sensorType, value);

            var history = _sensorValuesHistory.GetValues(sensorType);

            var anomaly = _anomalyDetector.Detect(
                sensorType,
                value,
                history,
                _options.WindowSize,
                _options.ZScoreThreshold);

            if (anomaly is null) continue;

            await _anomalyRepository.AddAsync(anomaly, cancellationToken);

            await _sensorHub.NotifyAnomalyAsync(ToDto(anomaly), cancellationToken);
        }
    }

    private static SensorReadingDto ToDto(SensorReading reading)
    {
        return new SensorReadingDto(reading.Id, reading.SequenceNumber, reading.Timestamp,
            reading.Temperature, reading.Humidity, reading.Co2Ppm);
    }

    private static AnomalyDto ToDto(Anomaly anomaly)
    {
        return new AnomalyDto(anomaly.Id, anomaly.DetectedAt, anomaly.SensorType,
            anomaly.Value, anomaly.ZScore, anomaly.Reason);
    }
}