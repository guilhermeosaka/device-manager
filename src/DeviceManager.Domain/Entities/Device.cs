using DeviceManager.Domain.Exceptions;
using DeviceManager.Domain.Types;

namespace DeviceManager.Domain.Entities;

// TODO: create an IAuditable interface with CreationTime to use with EF interceptors
public class Device  
{
    public required Guid Id { get; init; }
    public string? Name { get; private set; } // TODO: clarify whether null values should be allowed
    public string? Brand { get; private set; } // TODO: clarify whether null values should be allowed
    public StateType State { get; private set; } 
    public DateTimeOffset CreationTime { get; init; } // init prevents updating CreationTime 

    public static Device Create(
        string? name,
        string? brand,
        StateType state = StateType.Available,
        DateTimeOffset? creationTime = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Brand = brand,
            State = state,
            CreationTime = creationTime ?? DateTimeOffset.UtcNow // TODO: use EF interceptors to set this value
        };

    public void Rename(string? newName)
    {
        EnsureNotInUse(DeviceOperations.Rename);
        Name = newName;
    }
    
    public void Rebrand(string? newBrand)
    {
        EnsureNotInUse(DeviceOperations.Rebrand);
        Brand = newBrand;
    }

    public void ChangeState(StateType newState)
    {
        // TODO: check whether there are validation rules for state transitions
        State = newState;
    }

    public void EnsureCanBeDeleted()
    {
        EnsureNotInUse(DeviceOperations.Delete);
    }
    
    private void EnsureNotInUse(string operation)
    {
        if (State == StateType.InUse)
            throw new DeviceInUseException(operation);
    }
}