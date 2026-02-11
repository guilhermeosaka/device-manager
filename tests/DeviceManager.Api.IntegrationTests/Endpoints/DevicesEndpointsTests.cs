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
        
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<DeviceSummary>();
        response.Should().NotBeNull();
        
        response.Id.Should().Be(originalDevice.Id);
        response.Name.Should().Be(originalDevice.Name);
        response.Brand.Should().Be(originalDevice.Brand);
        response.State.Should().Be(originalDevice.State.ToString().Replace("-", " ").ToLowerInvariant());
        response.CreationTime.Should().Be(originalDevice.CreationTime);
    }
    
    #endregion Get
    
    #region List
    
    [Fact]
    public async Task List_NoFilter_Success()
    {
        // Arrange
        await ResetDbAsync();

        await CreateDeviceAsync(
            Device.Create("Name 1", "Brand 1"),
            Device.Create("Name 2", "Brand 1"),
            Device.Create("Name 3", "Brand 2"),
            Device.Create("Name 4", "Brand 2"),
            Device.Create("Name 5", "Brand 3"),
            Device.Create("Name 6", "Brand 3"));
    
        // Act
        var httpResponseMessage = await _client.GetAsync($"{BaseUrl}");
    
        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<PagedResponse<DeviceSummary>>();
        response.Should().NotBeNull();
        
        response.Items.Should().HaveCount(6);
    }
    
    [Theory]
    [InlineData("Brand 1", 1)]
    [InlineData("Brand 2", 2)]
    [InlineData("Brand 3", 3)]
    public async Task List_BrandFilter_Success(string brand, int expectedCount)
    {
        // Arrange
        await ResetDbAsync();

        await CreateDeviceAsync(
            Device.Create("Name 1", "Brand 1"),
            Device.Create("Name 2", "Brand 2"),
            Device.Create("Name 3", "Brand 2"),
            Device.Create("Name 4", "Brand 3"),
            Device.Create("Name 5", "Brand 3"),
            Device.Create("Name 6", "Brand 3"));
    
        // Act
        var httpResponseMessage = await _client.GetAsync($"{BaseUrl}?brand={brand}");
    
        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<PagedResponse<DeviceSummary>>();
        response.Should().NotBeNull();
        
        response.Items.Should().HaveCount(expectedCount);
    }
    
    [Theory]
    [InlineData("available", 3)]
    [InlineData("in-use", 2)]
    [InlineData("inactive", 1)]
    public async Task List_StateFilter_Success(string state, int expectedCount)
    {
        // Arrange
        await ResetDbAsync();

        await CreateDeviceAsync(
            Device.Create("Name 1", "Brand 1", StateType.Available),
            Device.Create("Name 2", "Brand 2", StateType.Available),
            Device.Create("Name 3", "Brand 2", StateType.Available),
            Device.Create("Name 4", "Brand 3", StateType.InUse),
            Device.Create("Name 5", "Brand 3", StateType.InUse),
            Device.Create("Name 6", "Brand 3", StateType.Inactive));
    
        // Act
        var httpResponseMessage = await _client.GetAsync($"{BaseUrl}?state={state}");
    
        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        
        var response = await httpResponseMessage.Content.ReadFromJsonAsync<PagedResponse<DeviceSummary>>();
        response.Should().NotBeNull();
        
        response.Items.Should().HaveCount(expectedCount);
    }

    #endregion List
    
    #region Delete
    
    [Fact]
    public async Task Delete_Success()
    {
        // Arrange
        await ResetDbAsync();

        var deviceToDelete = Device.Create("Deletable", "Deletable");
        var deviceToLeaveAlone = Device.Create("Leave alone", "Leave alone");
        await CreateDeviceAsync(deviceToDelete, deviceToLeaveAlone);
    
        // Act
        var httpResponseMessage = await _client.DeleteAsync($"{BaseUrl}/{deviceToDelete.Id}");
    
        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
        
        var deviceToDeleteExists = await db.Devices.AnyAsync(c => c.Id == deviceToDelete.Id);
        deviceToDeleteExists.Should().BeFalse();
        
        var deviceToLeaveAloneExists = await db.Devices.AnyAsync(c => c.Id == deviceToLeaveAlone.Id);
        deviceToLeaveAloneExists.Should().BeTrue();
    }
    
    #endregion Delete
    
    private async Task ResetDbAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
    
    private async Task CreateDeviceAsync(params Device[] devices)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();
        
        foreach (var device in devices)
            await db.Devices.AddAsync(device);
        
        await db.SaveChangesAsync();
    }
}