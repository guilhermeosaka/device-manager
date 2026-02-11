using DeviceManager.Application.Commands;
using DeviceManager.Domain.Abstractions;
using DeviceManager.Domain.Entities;
using DeviceManager.Domain.Types;

namespace DeviceManager.Application;

public class DevicesCommandHandler(IRepository<Device> deviceRepository, IUnitOfWork unitOfWork)
{
    public async Task<Guid> HandleAsync(CreateDeviceCommand command, CancellationToken ct = default)
    {
        var newDevice = Device.Create(command.Name, command.Brand, command.State ?? StateType.Available);
        await deviceRepository.AddAsync(newDevice, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return newDevice.Id;
    }
    
    public async Task HandleAsync(UpdateDeviceCommand command, CancellationToken ct = default)
    {
        var device = await deviceRepository.GetByIdAsync(command.Id);
        
        if (command.Name != null)
            device.Rename(command.Name);
        if (command.Brand != null)
            device.Rebrand(command.Brand);
        if (command.State != null)
            device.ChangeState(command.State.Value);
        
        await unitOfWork.SaveChangesAsync(ct);
    }
    
    public async Task HandleAsync(DeleteDeviceCommand command, CancellationToken ct = default)
    {
        var device = await deviceRepository.GetByIdAsync(command.Id);
        
        device.EnsureCanBeDeleted();
        
        deviceRepository.Remove(device);
        await unitOfWork.SaveChangesAsync(ct);
    }
}