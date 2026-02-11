using DeviceManager.Domain.Abstractions;

namespace DeviceManager.Infrastructure.Persistence;

public class UnitOfWork(DevicesDbContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default) => await dbContext.SaveChangesAsync(ct);
}