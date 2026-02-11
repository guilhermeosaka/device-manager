using DeviceManager.Domain.Types;

namespace DeviceManager.Application.Commands;

public record UpdateDeviceCommand(Guid Id, string? Name, string? Brand, StateType? State);