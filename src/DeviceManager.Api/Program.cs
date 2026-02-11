using DeviceManager.Api.ExceptionHandling;
using DeviceManager.Api.Extensions;
using DeviceManager.Application.Extensions;
using DeviceManager.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddPersistence(builder.Configuration.GetConnectionString("DevicesDb")!)
    .AddApplicationServices()
    .AddProblemDetails()
    .AddExceptionHandler<ExceptionHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunMigrationsAsync();

app.MapDeviceEndpoints();
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.Run();