using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeviceManager.Infrastructure.Persistence;

public class DbMigrator(IServiceScopeFactory scopeFactory, ILogger<DbMigrator> logger)
{
    public async Task MigrateAsync(CancellationToken ct = default)
    {
        await ApplyMigrationsAsync(ct);
    }

    private async Task ApplyMigrationsAsync(CancellationToken ct)
    {
        logger.LogInformation("Applying migrations...");
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
        await dbContext.Database.MigrateAsync(ct);
        logger.LogInformation("Migrations applied.");
    }
}