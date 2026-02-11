using DeviceManager.Domain.Types;

namespace DeviceManager.Application.Queries;

public record GetDevicesQuery(int Page, int PageSize, string? Brand, StateType? State);