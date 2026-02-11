namespace DeviceManager.Contracts.Requests;

public record UpdateDeviceRequest(string? Name, string? Brand, string? State);