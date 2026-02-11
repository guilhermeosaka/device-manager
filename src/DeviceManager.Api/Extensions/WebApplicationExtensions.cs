using DeviceManager.Application;
using DeviceManager.Application.Commands;
using DeviceManager.Contracts.Requests;
using DeviceManager.Contracts.Responses;
using DeviceManager.Domain.Types;
using DeviceManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Api.Extensions;

public static class WebApplicationExtensions
{
    // TODO: create a separate module and optionally use https://github.com/CarterCommunity/Carter
    public static void MapDeviceEndpoints(this WebApplication app)
    {
        const string prefix = "/devices";
        
        var statesMap = new Dictionary<string, StateType>
        {
            { "available", StateType.Available },
            { "in-use", StateType.InUse },
            { "inactive", StateType.Inactive }
        };

        var group = app.MapGroup(prefix);

        group.MapPost("/", async (CreateDeviceRequest request, DevicesCommandHandler handler, CancellationToken ct) =>
            {
                if (!statesMap.TryGetValue(request.State.ToLowerInvariant(), out var state))
                    return Results.Problem(
                        title: "Invalid state",
                        detail: $"Invalid device state: '{request.State}'. Use: {string.Join(", ", statesMap.Keys)}",
                        statusCode: StatusCodes.Status400BadRequest);

                var deviceId =
                    await handler.HandleAsync(new CreateDeviceCommand(request.Name, request.Brand, state), ct);
                return Results.Created($"{prefix}/{deviceId}", new CreateDeviceResponse(deviceId));
            })
            .Produces<CreateDeviceResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
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