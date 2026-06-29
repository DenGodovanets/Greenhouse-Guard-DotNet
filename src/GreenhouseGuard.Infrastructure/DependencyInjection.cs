using GreenhouseGuard.Application.Abstractions.Hubs;
using GreenhouseGuard.Application.Abstractions.Repositories;
using GreenhouseGuard.Application.Abstractions.Services;
using GreenhouseGuard.Infrastructure.BackgroundServices;
using GreenhouseGuard.Infrastructure.Hubs;
using GreenhouseGuard.Infrastructure.Persistence;
using GreenhouseGuard.Infrastructure.Repositories;
using GreenhouseGuard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreenhouseGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("GreenhouseGuard"));

        services.AddScoped<IReadingRepository, ReadingRepository>();
        services.AddScoped<IAnomalyRepository, AnomalyRepository>();
        services.AddScoped<ISensorHub, SensorHubNotifier>();
        services.AddSingleton<ISensorValuesHistory, InMemorySensorValuesHistory>();
        services.AddHostedService<SensorDataSimulator>();

        return services;
    }
}