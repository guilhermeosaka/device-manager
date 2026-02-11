using DeviceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Infrastructure.Persistence;

public class DevicesDbContext(DbContextOptions<DevicesDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevicesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}