using DeviceManager.Domain.Types;

namespace DeviceManager.Application.Commands;

public record CreateDeviceCommand(string? Name, string? Brand, StateType State);