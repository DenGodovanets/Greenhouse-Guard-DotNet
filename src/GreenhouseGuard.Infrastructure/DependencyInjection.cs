using GreenhouseGuard.Application.Abstractions.Services;
using GreenhouseGuard.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GreenhouseGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISensorValuesHistory, InMemorySensorValuesHistory>();

        return services;
    }
}
