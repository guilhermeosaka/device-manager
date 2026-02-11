namespace DeviceManager.Contracts.Requests;

public record CreateDeviceRequest(string? Name, string? Brand, string? State);