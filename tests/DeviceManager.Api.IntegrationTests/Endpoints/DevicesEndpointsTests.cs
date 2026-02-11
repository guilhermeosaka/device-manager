using System.Net.Http.Json;
using DeviceManager.Contracts.Requests;
using DeviceManager.Contracts.Responses;
using DeviceManager.Domain.Types;
using DeviceManager.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceManager.Api.IntegrationTests.Endpoints;

public class DevicesEndpointsTests(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private const string BaseUrl = "/devices";

    #region Create
    
    [Fact]
    public async Task Create_Success()
    {
        // Arrange
        var currentDate = DateTimeOffset.UtcNow;
        await ResetDbAsync();
        
        var request = new CreateDeviceRequest("Test name", "Test brand");
    
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
        device.State.Should().Be(StateType.Available);
        device.CreationTime.Should().BeOnOrAfter(currentDate);
    }
    
    #endregion Create
    
    private async Task ResetDbAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
}