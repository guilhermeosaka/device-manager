using DeviceManager.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceManager.Api.IntegrationTests.Extensions;

public static class ServiceCollectionExtensions
{
    public static void EnsureDbCreated(this IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
        db.Database.EnsureCreated();
    }
}