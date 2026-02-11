using DeviceManager.Domain.Entities;
using DeviceManager.Domain.Types;

namespace DeviceManager.Domain.Abstractions;

public interface IDevicesQueries
{
    Task<IReadOnlyList<Device>> GetAllAsync(
        int page,
        int pageSize,
        string? brand = null,
        StateType? state = null,
        CancellationToken ct = default);
}