using DeviceManager.Application;
using DeviceManager.Application.Commands;
using DeviceManager.Application.Queries;
using DeviceManager.Contracts.Requests;
using DeviceManager.Contracts.Responses;
using DeviceManager.Domain.Types;
using DeviceManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Api.Extensions;

public static class WebApplicationExtensions
{
    private static readonly Dictionary<string, StateType> StatesMap = new()
    {
        { "available", StateType.Available },
        { "in-use", StateType.InUse },
        { "inactive", StateType.Inactive }
    };
        
    private static readonly Dictionary<StateType, string> StatesUnmap = new()
    {
        { StateType.Available, "available" },
        { StateType.InUse, "in-use" },
        { StateType.Inactive, "inactive" }
    };
    
    // TODO: create a separate module and optionally use https://github.com/CarterCommunity/Carter
    public static void MapDeviceEndpoints(this WebApplication app)
    {
        const string prefix = "/devices";
        const int maxPageSize = 50;

        var group = app.MapGroup(prefix).ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("/", async (CreateDeviceRequest request, DevicesCommandHandler handler, CancellationToken ct) =>
            {
                if (!TryGetState(request.State, out var stateType))
                    return InvalidStateProblem(request.State!);

                var deviceId =
                    await handler.HandleAsync(new CreateDeviceCommand(request.Name, request.Brand, stateType), ct);
                return Results.Created($"{prefix}/{deviceId}", new CreateDeviceResponse(deviceId));
            })
            .Produces<CreateDeviceResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}",
                async (Guid id, UpdateDeviceRequest request, DevicesCommandHandler handler, CancellationToken ct) =>
                {
                    if (!TryGetState(request.State, out var stateType))
                        return InvalidStateProblem(request.State!);

                    await handler.HandleAsync(new UpdateDeviceCommand(id, request.Name, request.Brand, stateType), ct);

                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", async (Guid id, DevicesQueryHandler handler, CancellationToken ct) =>
            {
                var device = await handler.HandleAsync(new GetDeviceQuery(id), ct);
                return Results.Ok(new DeviceSummary(
                    device.Id,
                    device.Name,
                    device.Brand,
                    StatesUnmap[device.State],
                    device.CreationTime));
            })
            .Produces<DeviceSummary>();

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? brand,
            string? state,
            DevicesQueryHandler handler,
            CancellationToken ct) =>
        {
            if (!TryGetState(state, out var stateType))
                return InvalidStateProblem(state!);
            
            var devices = await handler.HandleAsync(
                new GetDevicesQuery(page ?? 1, pageSize ?? maxPageSize, brand, stateType), ct);
            
            var devicesSummaries = devices.Select(d => new DeviceSummary(
                d.Id,
                d.Name,
                d.Brand,
                StatesUnmap[d.State],
                d.CreationTime)).ToList();
            
            return Results.Ok(new PagedResponse<DeviceSummary>(devicesSummaries, devicesSummaries.Count));
        }).Produces<PagedResponse<DeviceSummary>>();

        group.MapDelete("/{id:guid}", async (Guid id, DevicesCommandHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(new DeleteDeviceCommand(id), ct);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);;
    }

    private static bool TryGetState(string? state, out StateType? stateType)
    {
        stateType = null;
        if (state == null)
            return true;

        if (!StatesMap.TryGetValue(state.ToLowerInvariant(), out var foundStateType))
            return false;

        stateType = foundStateType;
        return true;
    }
    
    private static IResult InvalidStateProblem(string state) =>
        Results.Problem(
            title: "Invalid state",
            detail:
            $"Invalid device state: '{state}'. Use: {string.Join(", ", StatesMap.Keys)}",
            statusCode: StatusCodes.Status400BadRequest);

    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        var runMigrations = app.Configuration.GetValue<bool>("RunDbMigrations");

        if (!runMigrations) return;

        using var scope = app.Services.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<DbMigrator>();
        await migrator.MigrateAsync();
    }
}