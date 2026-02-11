using System.Diagnostics.CodeAnalysis;
using DeviceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeviceManager.Infrastructure.Persistence.Configurations;

[ExcludeFromCodeCoverage]
public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");
        
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Id).HasColumnName("id").IsRequired();
        builder.Property(d => d.Name).HasColumnName("name");
        builder.Property(d => d.Brand).HasColumnName("brand");
        builder.Property(d => d.State).HasColumnName("state").IsRequired();
        builder.Property(d => d.CreationTime).HasColumnName("creation_time").IsRequired();
    }
}