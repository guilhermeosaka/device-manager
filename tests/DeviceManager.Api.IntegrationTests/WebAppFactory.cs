using DeviceManager.Api.IntegrationTests.Extensions;
using DeviceManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DeviceManager.Api.IntegrationTests;
 
 public class WebAppFactory : WebApplicationFactory<Program>
 {
     protected override void ConfigureWebHost(IWebHostBuilder builder)
     {
         builder.ConfigureAppConfiguration((_, config) =>
         {
             var overrides = new Dictionary<string, string>
             {
                 ["RunDbMigrations"] = "false"
             };

             config.AddInMemoryCollection(overrides!);
         });
         
         builder.ConfigureServices(services =>
         {
             services.RemoveAll<DbContextOptions<DevicesDbContext>>();
             services.RemoveAll<DbContextOptions>();
             services.RemoveAll<IDbContextOptionsConfiguration<DevicesDbContext>>();
             services.RemoveAll<DevicesDbContext>();
             
             var connection = new SqliteConnection("DataSource=:memory:");
             connection.Open();       
             
             services.AddDbContext<DevicesDbContext>(options => options.UseSqlite(connection));
 
             services.EnsureDbCreated();
         });
     }
 }