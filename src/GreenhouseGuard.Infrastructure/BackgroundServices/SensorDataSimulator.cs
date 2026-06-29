using GreenhouseGuard.Application.Features.Readings.Commands.CreateReading;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenhouseGuard.Infrastructure.BackgroundServices;

public sealed class SensorDataSimulator : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(3);
    private readonly ILogger<SensorDataSimulator> _logger;
    private readonly Random _random = new();

    private readonly IServiceScopeFactory _scopeFactory;
    private int _baseCo2 = 810;
    private decimal _baseHumidity = 62m;

    private decimal _baseTemperature = 22m;
    private int _tickCount;

    public SensorDataSimulator(IServiceScopeFactory scopeFactory, ILogger<SensorDataSimulator> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SensorDataSimulator started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                var command = BuildCommand();
                await SendAsync(command, stoppingToken);

                _logger.LogDebug(
                    "Simulated reading: Temp={Temperature}, Humidity={Humidity}, CO2={Co2}",
                    command.Temperature, command.Humidity, command.Co2Ppm);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to simulate sensor reading");
            }
        }

        _logger.LogInformation("SensorDataSimulator stopped");
    }

    private CreateReadingCommand BuildCommand()
    {
        _tickCount++;

        _baseTemperature += (decimal)(_random.NextDouble() * 0.1 - 0.05);
        _baseHumidity += (decimal)(_random.NextDouble() * 0.2 - 0.1);
        _baseCo2 += _random.Next(-3, 4);

        _baseTemperature = Math.Clamp(_baseTemperature, 18m, 28m);
        _baseHumidity = Math.Clamp(_baseHumidity, 50m, 75m);
        _baseCo2 = Math.Clamp(_baseCo2, 700, 950);

        var temperature = _baseTemperature + (decimal)(_random.NextDouble() * 0.4 - 0.2);
        var humidity = _baseHumidity + (decimal)(_random.NextDouble() * 1.0 - 0.5);
        var co2 = _baseCo2 + _random.Next(-10, 11);

        if (_tickCount % 30 == 0)
        {
            var spike = _random.Next(0, 3);
            temperature = spike == 0 ? _baseTemperature + 15m : temperature;
            humidity = spike == 1 ? _baseHumidity + 20m : humidity;
            co2 = spike == 2 ? _baseCo2 + 300 : co2;
        }

        return new CreateReadingCommand(
            Math.Round(temperature, 2),
            Math.Round(humidity, 2),
            co2);
    }

    private async Task SendAsync(CreateReadingCommand command, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(command, cancellationToken);
    }
}