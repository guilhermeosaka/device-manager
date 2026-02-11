namespace DeviceManager.Domain.Abstractions;

public interface IRepository<in T>
{
    Task AddAsync(T entity, CancellationToken ct = default);
}