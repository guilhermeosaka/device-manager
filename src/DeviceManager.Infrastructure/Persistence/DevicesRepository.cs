using DeviceManager.Domain.Abstractions;
using DeviceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Infrastructure.Persistence;

// TODO: create a generic Repository implementation (also, make Device inherit from Entity)
public class DevicesRepository(DevicesDbContext dbContext) : IRepository<Device>
{
    public async Task AddAsync(Device entity, CancellationToken ct = default) =>
        await dbContext.Devices.AddAsync(entity, ct);

    public async Task<Device> GetByIdAsync(Guid id) => await dbContext.Devices.SingleAsync(d => d.Id == id);
    
    // TODO: check whether device should be soft deleted instead (currently it's a hard delete)
    public void Remove(Device item) => dbContext.Devices.Remove(item);
}