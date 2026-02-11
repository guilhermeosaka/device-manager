namespace DeviceManager.Domain.Exceptions;

public class DeviceInUseException(string operation) : Exception($"Cannot {operation} a device that is in use.");