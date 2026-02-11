using DeviceManager.Domain.Abstractions;

namespace DeviceManager.Infrastructure.Persistence;

public class Repository<T>(DevicesDbContext dbContext) : IRepository<T> where T : class
{
    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await dbContext.Set<T>().AddAsync(entity, ct);
}