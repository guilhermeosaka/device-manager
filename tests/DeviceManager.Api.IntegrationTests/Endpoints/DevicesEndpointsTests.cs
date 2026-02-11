using System.Net;
using System.Net.Http.Json;
using DeviceManager.Contracts.Requests;
using DeviceManager.Contracts.Responses;
using DeviceManager.Domain.Entities;
using DeviceManager.Domain.Types;
using DeviceManager.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceManager.Api.IntegrationTests.Endpoints;

public class DevicesEndpointsTests(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private const string BaseUrl = "/devices";

    #region Create
    
    [Theory]
    [InlineData("AVAILABLE", StateType.Available)]
    [InlineData("in-use", StateType.InUse)]
    [InlineData("InAcTiVe", StateType.Inactive)]
    public async Task Create_Success(string state, StateType expectedState)
    {
        // Arrange
        var currentDate = DateTimeOffset.UtcNow;
        await ResetDbAsync();
        
        var request = new CreateDeviceRequest("Test name", "Test brand", state);
    
        // Act
        var httpResponseMessage = await _client.PostAsJsonAsync(BaseUrl, request);
    
        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<CreateDeviceResponse>();
        response.Should().NotBeNull();
        
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
        
        var device = await db.Devices.FirstOrDefaultAsync(c => c.Id == response.Id);

        device.Should().NotBeNull();
        device.Name.Should().Be(request.Name);
        device.Brand.Should().Be(request.Brand);
        device.State.Should().Be(expectedState);
        device.CreationTime.Should().BeOnOrAfter(currentDate);
    }
    
    [Theory]
    [InlineData("unknown")]
    [InlineData("inuse")]
    [InlineData("active")]
    public async Task Create_Failure(string state)
    {
        // Arrange
        await ResetDbAsync();
        
        var request = new CreateDeviceRequest("Test name", "Test brand", state);
    
        // Act
        var httpResponseMessage = await _client.PostAsJsonAsync(BaseUrl, request);
    
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problemDetails = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Title.Should().Be("Invalid state");
        problemDetails.Detail.Should().Be($"Invalid device state: '{state}'. Use: available, in-use, inactive");
    }
    
    #endregion Create
    
    #region Update
    
    [Theory]
    [InlineData("New name", "New brand", "in-use", StateType.InUse)]
    [InlineData(null, "New brand", "in-use", StateType.InUse)]
    [InlineData("New name", null, "in-use", StateType.InUse)]
    [InlineData("New name", "New brand", null, null)]
    public async Task Update_Success(string? newName, string? newBrand, string? newState, StateType? newStateEnum)
    {
        // Arrange
        await ResetDbAsync();

        var originalDevice = Device.Create("Test name", "Test brand");
        await CreateDeviceAsync(originalDevice);
        
        var currentDate = DateTimeOffset.UtcNow;
        var request = new UpdateDeviceRequest(newName, newBrand, newState);
    
        // Act
        var httpResponseMessage = await _client.PutAsJsonAsync($"{BaseUrl}/{originalDevice.Id}", request);
    
        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
        
        var updatedDevice = await db.Devices.FirstOrDefaultAsync(c => c.Id == originalDevice.Id);

        updatedDevice.Should().NotBeNull();
        updatedDevice.Name.Should().Be(newName ?? originalDevice.Name);
        updatedDevice.Brand.Should().Be(newBrand ?? originalDevice.Brand);
        updatedDevice.State.Should().Be(newState != null ? newStateEnum : originalDevice.State);
        updatedDevice.CreationTime.Should().BeOnOrBefore(currentDate);
    }
    
    [Theory]
    [InlineData("unknown")]
    [InlineData("inuse")]
    [InlineData("active")]
    public async Task Update_BadState_Failure(string state)
    {
        // Arrange
        await ResetDbAsync();
        
        var originalDevice = Device.Create("Test name", "Test brand");
        var request = new CreateDeviceRequest("Test name", "Test brand", state);
    
        // Act
        var httpResponseMessage = await _client.PutAsJsonAsync($"{BaseUrl}/{originalDevice.Id}", request);
    
        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problemDetails = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Title.Should().Be("Invalid state");
        problemDetails.Detail.Should().Be($"Invalid device state: '{state}'. Use: available, in-use, inactive");
    }
    
    #endregion Update
    
    #region Get 
    
    [Fact]
    public async Task Get_Success()
    {
        // Arrange
        await ResetDbAsync();

        var originalDevice = Device.Create("Test name", "Test brand");
        await CreateDeviceAsync(originalDevice);
    
        // Act
        var httpResponseMessage = await _client.GetAsync($"{BaseUrl}/{originalDevice.Id}");
    
        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<GetDeviceResponse>();
        response.Should().NotBeNull();
        
        response.Id.Should().Be(originalDevice.Id);
        response.Name.Should().Be(originalDevice.Name);
        response.Brand.Should().Be(originalDevice.Brand);
        response.State.Should().Be(originalDevice.State.ToString().Replace("-", " ").ToLowerInvariant());
        response.CreationTime.Should().Be(originalDevice.CreationTime);
    }
    
    #endregion
    
    private async Task ResetDbAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
    
    private async Task CreateDeviceAsync(Device device)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
        
        await db.Devices.AddAsync(device);
        await db.SaveChangesAsync();
    }
}