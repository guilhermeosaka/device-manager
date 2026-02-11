using Microsoft.Extensions.DependencyInjection;

namespace DeviceManager.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services
            .AddScoped<DevicesCommandHandler>()
            .AddScoped<DevicesQueryHandler>();
        
        return services;
    }
}