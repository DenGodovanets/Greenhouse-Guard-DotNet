using GreenhouseGuard.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GreenhouseGuard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddSingleton<AnomalyDetector>();

        return services;
    }
}