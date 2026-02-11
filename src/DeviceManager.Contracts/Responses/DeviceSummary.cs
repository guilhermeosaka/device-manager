namespace DeviceManager.Contracts.Responses;

public record DeviceSummary(Guid Id, string? Name, string? Brand, string State, DateTimeOffset CreationTime);