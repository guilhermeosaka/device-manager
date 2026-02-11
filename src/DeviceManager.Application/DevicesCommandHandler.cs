using DeviceManager.Application.Commands;
using DeviceManager.Domain.Abstractions;
using DeviceManager.Domain.Entities;

namespace DeviceManager.Application;

public class DevicesCommandHandler(IRepository<Device> deviceRepository, IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(CreateDeviceCommand command, CancellationToken ct = default)
    {
        var newDevice = Device.Create(command.Name, command.Brand);
        await deviceRepository.AddAsync(newDevice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return newDevice.Id;
    }
}