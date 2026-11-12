using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql;
using QTKD.Api.Data;
using Xunit;

namespace QTKD.Api.Tests.Integration;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _connectionString = LoadConnection();

    private static string LoadConnection()
    {
        var value = Environment.GetEnvironmentVariable("QTKD_TEST_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(value))
            value = new ConfigurationBuilder().AddUserSecrets<ApiFactory>()
                .Build().GetConnectionString("QtkdTestDatabase");
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Set QTKD_TEST_CONNECTION_STRING or test User Secrets ConnectionStrings:QtkdTestDatabase for qtkd_test.");
        try
        {
            if (new NpgsqlConnectionStringBuilder(value).Database != "qtkd_test")
                throw new InvalidOperationException("Integration tests require Database=qtkd_test. No database was changed.");
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("Invalid test connection configuration. Configure a valid PostgreSQL connection for qtkd_test.");
        }
        return value;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:QtkdDatabase", _connectionString);
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<QtkdDbContext>();
            services.RemoveAll<DbContextOptions<QtkdDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<QtkdDbContext>>();
            services.AddDbContext<QtkdDbContext>(options => options.UseNpgsql(_connectionString));
        });
    }

    public QtkdDbContext NewContext() => new(new DbContextOptionsBuilder<QtkdDbContext>()
        .UseNpgsql(_connectionString).Options);

    public async Task InitializeAsync()
    {
        await using var db = NewContext();
        await db.Database.MigrateAsync();
    }

    Task IAsyncLifetime.DisposeAsync() => base.DisposeAsync().AsTask();
}
