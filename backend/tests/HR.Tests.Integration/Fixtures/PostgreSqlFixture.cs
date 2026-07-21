namespace HR.Tests.Integration.Fixtures;

/// <summary>
/// PostgreSQL container fixture for integration tests.
/// Implements IAsyncLifetime for xUnit collection setup/teardown.
/// </summary>
public class PostgreSqlFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    
    public string? ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("hr_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    public DbContextOptions<TDbContext> GetDbContextOptions<TDbContext>()
        where TDbContext : DbContext
    {
        if (ConnectionString is null)
            throw new InvalidOperationException("PostgreSQL container not initialized");

        return new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
    }
}

/// <summary>
/// Collection definition for PostgreSQL tests.
/// </summary>
[CollectionDefinition("PostgreSQL Collection")]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    // This class has no code, and is never created. Its purpose is only
    // to define the collection that this test class belongs to.
    // It is used by placing the [Collection("PostgreSQL Collection")]
    // attribute on the test class.
}
