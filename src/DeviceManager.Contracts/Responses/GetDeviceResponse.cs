namespace DeviceManager.Contracts.Responses;

public record GetDeviceResponse(Guid Id, string? Name, string? Brand, string State, DateTimeOffset CreationTime);