using DeviceManager.Application.Queries;
using DeviceManager.Domain.Abstractions;
using DeviceManager.Domain.Entities;

namespace DeviceManager.Application;

public class DevicesQueryHandler(IRepository<Device> deviceRepository)
{
    public async Task<Device> HandleAsync(GetDeviceQuery query, CancellationToken ct = default) =>
        await deviceRepository.GetByIdAsync(query.Id);
}