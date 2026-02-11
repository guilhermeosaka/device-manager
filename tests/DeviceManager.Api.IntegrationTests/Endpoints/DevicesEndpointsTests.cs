using System.Net;
using System.Net.Http.Json;
using DeviceManager.Contracts.Requests;
using DeviceManager.Contracts.Responses;
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
    
    private async Task ResetDbAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DevicesDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
}