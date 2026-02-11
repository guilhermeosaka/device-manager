namespace DeviceManager.Contracts.Responses;

public record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount);