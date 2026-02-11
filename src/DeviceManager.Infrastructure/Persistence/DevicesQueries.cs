using DeviceManager.Domain.Abstractions;
using DeviceManager.Domain.Entities;
using DeviceManager.Domain.Types;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Infrastructure.Persistence;

public class DevicesQueries(DevicesDbContext dbContext) : IDevicesQueries
{
    public async Task<IReadOnlyList<Device>> GetAllAsync(
        int page, 
        int pageSize, 
        string? brand = null,
        StateType? state = null,
        CancellationToken ct = default)
    {
        var query = dbContext.Devices.AsQueryable();
        
        if (brand != null)
            query = query.Where(d => d.Brand == brand);
        
        if (state != null)
            query = query.Where(d => d.State == state);

        return await query
            .OrderByDescending(d => d.CreationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: ct);
    }
}