using DeviceManager.Domain.Entities;
using DeviceManager.Domain.Exceptions;
using DeviceManager.Domain.Types;

namespace DeviceManager.Domain.Tests.Entities;

public class DeviceTests
{
    #region Rename
    
    [Theory]
    [InlineData(StateType.Available)]
    [InlineData(StateType.Inactive)]
    public void Rename_Success(StateType currentState)
    {
        // Arrange
        const string newName = "New name";
        var device = Device.Create("Old name", "Old brand", currentState);
        
        // Act
        device.Rename(newName);
        
        // Assert
        Assert.Equal(newName, device.Name);
    }
    
    [Fact]
    public void Rename_Failure()
    {
        // Arrange
        const string expectedError = "Cannot rename a device that is in use.";
        var device = Device.Create("Old name", "Old brand", StateType.InUse);
        
        // Act & Assert
        var exception = Assert.Throws<DeviceInUseException>(() => device.Rename("New name"));
        Assert.Equal(expectedError, exception.Message);
    }
    
    #endregion Rename
    
    #region Rebrand
    
    [Theory]
    [InlineData(StateType.Available)]
    [InlineData(StateType.Inactive)]
    public void Rebrand_Success(StateType currentState)
    {
        // Arrange
        const string newBrand = "New name";
        var device = Device.Create("Old name", "Old brand", currentState);
        
        // Act
        device.Rebrand(newBrand);
        
        // Assert
        Assert.Equal(newBrand, device.Brand);
    }
    
    [Fact]
    public void Rebrand_Failure()
    {
        // Arrange
        const string expectedError = "Cannot rebrand a device that is in use.";
        var device = Device.Create("Old name", "Old brand", StateType.InUse);
        
        // Act & Assert
        var exception = Assert.Throws<DeviceInUseException>(() => device.Rebrand("New brand"));
        Assert.Equal(expectedError, exception.Message);
    }
    
    #endregion Rebrand
    
    #region ChangeState

    [Theory]
    [InlineData(StateType.Available, StateType.InUse)]
    [InlineData(StateType.Available, StateType.Inactive)]
    [InlineData(StateType.InUse, StateType.Available)]
    [InlineData(StateType.InUse, StateType.Inactive)]
    [InlineData(StateType.Inactive, StateType.Available)]
    [InlineData(StateType.Inactive, StateType.InUse)]
    public void ChangeState_Success(StateType currentState, StateType newState)
    {
        // Arrange
        var device = Device.Create("Test name", "Test brand", currentState);
        
        // Act
        device.ChangeState(newState);
        
        // Assert
        Assert.Equal(newState, device.State);
    }
    
    #endregion ChangeState
    
    #region EnsureCanBeDeleted

    [Theory]
    [InlineData(StateType.Available)]
    [InlineData(StateType.Inactive)]
    public void EnsureCanBeDeleted_Success(StateType currentState)
    {
        // Arrange
        var device = Device.Create("Name", "Brand", currentState);
        
        // Act
        device.EnsureCanBeDeleted();
    }

    [Fact]
    public void EnsureCanBeDeleted_Failure()
    {
        // Arrange
        const string expectedError = "Cannot delete a device that is in use.";
        var device = Device.Create("Name", "Brand", StateType.InUse);

        // Act & Assert
        var exception = Assert.Throws<DeviceInUseException>(() => device.EnsureCanBeDeleted());
        Assert.Equal(expectedError, exception.Message);
    }
    
    #endregion EnsureCanBeDeleted
}