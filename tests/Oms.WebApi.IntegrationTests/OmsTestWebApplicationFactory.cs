using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oms.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Oms.WebApi.IntegrationTests;

// We implement IAsyncLifetime so xUnit knows to start the database before tests run,
// and stop/dispose of it when the tests finish.
public class OmsTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Declare the PostgreSqlContainer instance
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine") // 👈 Lightweight official Postgres image
        .WithDatabase("oms_test")
        .WithUsername("postgres")
        .WithPassword("postgres_test_password")
        .Build();

    // 1. Starts the Docker container
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    // 2. Intercepts the Web API setup to swap out the connection string
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Find and remove the existing database context registration (which points to your real DB)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OmsDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Register OmsDbContext to connect to our dynamic Testcontainer Postgres instead!
            services.AddDbContext<OmsDbContext>(options =>
            {
                // _dbContainer.GetConnectionString() generates the exact host/port Docker assigned
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            // Automatically run EF Core Migrations to set up tables in the test container
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OmsDbContext>();
            db.Database.Migrate(); // 👈 Applies your migrations to the fresh test container database
        });
    }

    // 3. Shuts down and deletes the Docker container when all tests are done
    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}