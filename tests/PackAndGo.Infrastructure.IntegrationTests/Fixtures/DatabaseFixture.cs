using Microsoft.EntityFrameworkCore;
using PackAndGo.Infrastructure.Persistence;

namespace PackAndGo.Infrastructure.IntegrationTests.Fixtures;

public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        Context = new AppDbContext(options);

        // Optional: Seed initial data
        SeedData();
    }

    private void SeedData()
    {
        // Seed initial data for testing, if necessary
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
