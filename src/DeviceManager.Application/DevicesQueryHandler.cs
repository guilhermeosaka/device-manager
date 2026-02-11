using DeviceManager.Application.Queries;
using DeviceManager.Domain.Abstractions;
using DeviceManager.Domain.Entities;

namespace DeviceManager.Application;

public class DevicesQueryHandler(IRepository<Device> deviceRepository, IDevicesQueries devicesQueries)
{
    public async Task<Device> HandleAsync(GetDeviceQuery query, CancellationToken ct = default) =>
        await deviceRepository.GetByIdAsync(query.Id);
    
    public async Task<IReadOnlyList<Device>> HandleAsync(GetDevicesQuery query, CancellationToken ct = default) =>
        await devicesQueries.GetAllAsync(query.Page, query.PageSize, query.Brand, query.State, ct);
}