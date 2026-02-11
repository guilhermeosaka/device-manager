namespace DeviceManager.Domain.Abstractions;

public interface IRepository<T>
{
    Task AddAsync(T entity, CancellationToken ct = default);
    
    Task<T> GetByIdAsync(Guid id);
}