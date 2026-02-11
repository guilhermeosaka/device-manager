using DeviceManager.Application;
using DeviceManager.Application.Commands;
using DeviceManager.Contracts.Requests;
using DeviceManager.Contracts.Responses;
using DeviceManager.Infrastructure.Persistence;

namespace DeviceManager.Api.Extensions;

public static class WebApplicationExtensions
{
    // TODO: create a separate module and optionally use https://github.com/CarterCommunity/Carter
    public static void MapDeviceEndpoints(this WebApplication app)
    {
        const string prefix = "/devices";
        
        var group = app.MapGroup(prefix);

        group.MapPost("/", async (CreateDeviceRequest request, DevicesCommandHandler handler, CancellationToken ct) =>
            {
                var deviceId = await handler.HandleAsync(new CreateDeviceCommand(request.Name, request.Brand), ct);
                return Results.Created($"{prefix}/{deviceId}", new CreateDeviceResponse(deviceId));
            })
            .Produces<CreateDeviceResponse>(StatusCodes.Status201Created);
    }

    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        var runMigrations = app.Configuration.GetValue<bool>("RunDbMigrations");
        
        if (!runMigrations) return;
        
        using var scope = app.Services.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<DbMigrator>();
        await migrator.MigrateAsync();
    }
}