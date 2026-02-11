using DeviceManager.Domain.Abstractions;
using DeviceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceManager.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services
            .AddDbContext<DevicesDbContext>(options => options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(3),
                    errorCodesToAdd: null);
            }))
            .AddScoped(typeof(IRepository<>), typeof(Repository<>))
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<DbMigrator>();

        return services;
    }
}