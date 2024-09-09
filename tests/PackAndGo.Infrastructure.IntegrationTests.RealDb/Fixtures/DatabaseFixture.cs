using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PackAndGo.Infrastructure.Persistence;

namespace PackAndGo.Infrastructure.IntegrationTests.RealDb.Fixtures
{
    public class DatabaseFixture : IDisposable
    {
        public AppDbContext Context { get; private set; }
        private readonly SqliteConnection _connection;
        private readonly string _dbPath;

        public DatabaseFixture()
        {
            // Generate a unique database file name for each test class
            _dbPath = Path.Combine(Directory.GetCurrentDirectory(), $"test.{Guid.NewGuid()}.db");

            _connection = new SqliteConnection($"Data Source={_dbPath}");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new AppDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Close();

            // Clean up the database file after the test
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
    }
}
