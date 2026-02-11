using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DeviceManager.Api.IntegrationTests.Customizers;

public class SqliteModelCustomizer : IModelCustomizer
{
    public void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var props = entity.GetProperties()
                .Where(p => p.ClrType == typeof(DateTimeOffset));

            foreach (var prop in props)
            {
                prop.SetValueConverter(new ValueConverter<DateTimeOffset, string>(
                    v => v.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffffK"),
                    v => DateTimeOffset.Parse(v)
                ));
            }
        }
    }
}