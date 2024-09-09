### Integration Testing the Infrastructure Layer with SQLite

In this chapter, we will explore how to set up and perform **integration tests** for the infrastructure layer using a real SQLite database. These tests will validate that the repository layer interacts correctly with the database. We’ll also ensure that the tests only run conditionally based on an environment variable, providing a clean separation between unit and integration testing.

### 1. Setting Up the Integration Test Project

To begin, we’ll create a separate project for our integration tests, ensuring that the tests only run when a specific environment variable is set.

#### 1.1 Create the Test Project

Create the test project for integration tests using SQLite:

```bash
dotnet new xunit -o tests/PackAndGo.Infrastructure.IntegrationTests.RealDb
dotnet add tests/PackAndGo.Infrastructure.IntegrationTests.RealDb reference src/PackAndGo.Infrastructure
dotnet add tests/PackAndGo.Infrastructure.IntegrationTests.RealDb reference src/PackAndGo.Domain
dotnet sln add tests/PackAndGo.Infrastructure.IntegrationTests.RealDb/PackAndGo.Infrastructure.IntegrationTests.RealDb.csproj
```

#### 1.2 Add Necessary Packages

We need to add SQLite and EF Core packages for testing:

```bash
dotnet add tests/PackAndGo.Infrastructure.IntegrationTests.RealDb package Microsoft.EntityFrameworkCore.Sqlite
dotnet add tests/PackAndGo.Infrastructure.IntegrationTests.RealDb package Microsoft.EntityFrameworkCore.Design
```

---

### 2. Configuring the Database Fixture

A database fixture will be used to manage the lifecycle of the SQLite database during tests. This fixture will handle database creation, disposal, and ensuring the database is set up correctly.

#### `DatabaseFixture.cs`

```csharp
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
```

#### **Explanation**:
- **Database Management**: The fixture sets up an SQLite database and ensures it is correctly disposed of and deleted after each test run.
- **Database Isolation**: Each test uses a unique SQLite database file to prevent interference between tests.

---

### 3. Conditional Test Execution

To ensure these integration tests only run when a specific environment variable is set, we create a custom attribute. This attribute will check for the `USE_REAL_DATABASE` environment variable and skip the tests if it's not set.

#### `RealDatabaseFactAttribute.cs`

```csharp
using Xunit;

namespace PackAndGo.Infrastructure.IntegrationTests.RealDb;

public class RealDatabaseFactAttribute : FactAttribute
{
    public RealDatabaseFactAttribute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USE_REAL_DATABASE")))
        {
            Skip = "Skipping test because USE_REAL_DATABASE is not set.";
        }
    }
}
```

#### **Explanation**:
- **Conditional Execution**: This custom `Fact` attribute ensures that the test is only executed when the environment variable `USE_REAL_DATABASE` is set. If the variable is not set, the test will be skipped, making it easy to separate unit and integration tests.

---

### 4. Test Collection for Sequential Execution

Since SQLite doesn’t support parallel writes well, we need to ensure that our tests run sequentially. We use the `Collection` and `CollectionDefinition` attributes to disable parallel test execution.

#### `SequentialTestCollection.cs`

```csharp
using PackAndGo.Infrastructure.IntegrationTests.RealDb.Fixtures;
using Xunit;

[CollectionDefinition("Sequential Test Collection", DisableParallelization = true)]
public class SequentialTestCollection : ICollectionFixture<DatabaseFixture>
{
}
```

#### **Explanation**:
- **Sequential Execution**: This ensures that all tests within the `Sequential Test Collection` run one after another, preventing parallel execution conflicts with the SQLite database.

---

### 5. User Repository Integration Tests

Now that the test infrastructure is ready, let’s write integration tests for the `UserRepository`.

#### `UserRepositoryTests.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using PackAndGo.Domain.Entities;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.IntegrationTests.RealDb.Fixtures;
using Xunit;
using System.Runtime.CompilerServices;

namespace PackAndGo.Infrastructure.IntegrationTests.RealDb.Repositories;


[Collection("Sequential Test Collection")] // Ensures the tests run sequentially
public class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _dbFixture;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _dbFixture = fixture;
        _userRepository = new UserRepository(_dbFixture.Context);
    }

    private static void PauseTest([CallerMemberName] string methodName = "")
    {
        // Check if the environment variable is set
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ENABLE_MANUAL_TEST_PAUSE")))
        {
            return;  // Skip pausing if the environment variable is not set
        }

        // Pause the test and display the method name
        Console.WriteLine($"Method: {methodName}");
        Console.WriteLine("Test paused. Press Enter to continue...");
        Console.ReadLine();
    }

    [RealDatabaseFact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = User.Create("newuser@example.com");

        // Act
        await _userRepository.AddAsync(user);
        var result = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newuser@example.com", result.Email.Value);

        PauseTest();
    }

    [RealDatabaseFact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = User.Create("anothernewuser@example.com");
        await _userRepository.AddAsync(user);

        // Act
        var result = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("anothernewuser@example.com", result.Email.Value);
        PauseTest();
    }

    [RealDatabaseFact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var nonExcistingUserId = Guid.NewGuid();
        var result = await _userRepository.GetByIdAsync(nonExcistingUserId);

        // Assert
        Assert.Null(result);
        PauseTest();
    }

    [RealDatabaseFact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = User.Create("user1@example.com");
        var user2 = User.Create("user2@example.com");
        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);

        // Act
        var users = await _userRepository.GetAllAsync();

        // Assert
        Assert.NotNull(users);
        Assert.True(users.Count() >= 2); // The database may be seeded with additional users
        Assert.Contains(users, u => u.Email.Value == "user1@example.com");
        Assert.Contains(users, u => u.Email.Value == "user2@example.com");
        PauseTest();
    }

    [RealDatabaseFact]
    public async Task UpdateAsync_ShouldUpdateUserEmail()
    {
        // Arrange
        var user = User.Create("original@example.com");
        await _userRepository.AddAsync(user);
        PauseTest();

        // Act
        user.ChangeEmail("updated@example.com");
        await _userRepository.UpdateAsync(user);
        var result = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updated@example.com", result.Email.Value);
        PauseTest();
    }

    [RealDatabaseFact]
    public async Task DeleteAsync_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var user = User.Create("usertodelete@example.com");
        await _userRepository.AddAsync(user);
        PauseTest();

        // Act
        await _userRepository.DeleteAsync(user.Id);
        var result = await _userRepository.GetByIdAsync(user.Id);

        // Assert
        Assert.Null(result);
        PauseTest();
    }
}
```

#### **Explanation**:
- **CRUD Operations**: These tests verify that all CRUD operations (`Add`, `Get`, `Update`, `Delete`) are performed correctly in the `UserRepository` when interacting with a real SQLite database.
- **Pause for Debugging**: The `PauseTest` method allows manual inspection of the database between tests. You can enable this feature by setting the environment variable `ENABLE_MANUAL_TEST_PAUSE`.
- **Sequential Execution**: The tests run sequentially to prevent issues with the SQLite database.

---

### 6. PackingList Repository Integration Tests

Next, we’ll test the `PackingListRepository` to ensure it correctly interacts with the SQLite database.

#### `PackingListRepositoryTests.cs`

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.IntegrationTests.RealDb.Fixtures;
using Xunit;
using System.Runtime.CompilerServices;

namespace PackAndGo.Infrastructure.IntegrationTests.RealDb.Repositories
{
    [Collection("Sequential Test Collection")] // Ensures the tests run sequentially
    public class PackingListRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _dbFixture;
        private readonly PackingListRepository _packingListRepository;

        public PackingListRepositoryTests(DatabaseFixture fixture)
        {
            _dbFixture = fixture;
            _packingListRepository = new PackingListRepository(_dbFixture.Context);
        }

        private static void PauseTest([CallerMemberName] string methodName = "")
        {
            // Check if the environment variable is set
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ENABLE_MANUAL_TEST_PAUSE")))
            {
                return;  // Skip pausing if the environment variable is not set
            }

            // Pause the test and display the method name
            Console.WriteLine($"Method: {methodName}");
            Console.WriteLine("Test paused. Press Enter to continue...");
            Console.ReadLine();
        }

        [RealDatabaseFact]
        public async Task AddAsync_ShouldAddPackingListToDatabase()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());

            // Act
            await _packingListRepository.AddAsync(packingList);
            var result = await _packingListRepository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Vacation", result.Name);
            Assert.Empty(result.Items);

            PauseTest();
        }

        [RealDatabaseFact]
        public async Task GetByIdAsync_ShouldReturnPackingList_WhenItExists()
        {
            // Arrange
            var packingList = PackingList.Create("Business Trip", Guid.NewGuid());
            await _packingListRepository.AddAsync(packingList);

            // Act
            var result = await _packingListRepository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packingList.Id, result.Id);
            Assert.Equal("Business Trip", result.Name);
            PauseTest();
        }

        [RealDatabaseFact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenPackingListDoesNotExist()
        {
            // Act
            var nonExistingPackingListId = Guid.NewGuid();
            var result = await _packingListRepository.GetByIdAsync(nonExistingPackingListId);

            // Assert
            Assert.Null(result);
            PauseTest();
        }

        [RealDatabaseFact]
        public async Task GetAllByOwnerIdAsync_ShouldReturnAllPackingListsForOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var packingList1 = PackingList.Create("List 1", ownerId);
            var packingList2 = PackingList.Create("List 2", ownerId);
            await _packingListRepository.AddAsync(packingList1);
            await _packingListRepository.AddAsync(packingList2);

            // Act
            var packingLists = await _packingListRepository.GetAllByOwnerIdAsync(ownerId);

            // Assert
            Assert.NotNull(packingLists);
            Assert.Equal(2, packingLists.Count());
            Assert.Contains(packingLists, pl => pl.Name == "List 1");
            Assert.Contains(packingLists, pl => pl.Name == "List 2");
            PauseTest();
        }

        [RealDatabaseFact]
        public async Task UpdateAsync_ShouldUpdatePackingListAndItems()
        {
            // Arrange
            var packingList = PackingList.Create("Old Name", Guid.NewGuid());
            packingList.AddItem("Old Item");
            await _packingListRepository.AddAsync(packingList);
            PauseTest();

            // Act
            packingList.ChangeName("New Name");
            packingList.ChangeItemName(packingList.Items.First().Id, "New Item");
            packingList.AddItem("Second Item");
            await _packingListRepository.UpdateAsync(packingList);
            var result = await _packingListRepository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Name", result.Name);
            Assert.Equal(2, result.Items.Count);
            Assert.Contains(result.Items, i => i.Name == "New Item");
            Assert.Contains(result.Items, i => i.Name == "Second Item");
            PauseTest();
        }

        [RealDatabaseFact]
        public async Task DeleteAsync_ShouldRemovePackingListFromDatabase()
        {
            // Arrange
            var packingList = PackingList.Create("List to Delete", Guid.NewGuid());
            await _packingListRepository.AddAsync(packingList);
            PauseTest();

            // Act
            await _packingListRepository.DeleteAsync(packingList.Id);
            var result = await _packingListRepository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.Null(result);
            PauseTest();
        }
    }
}
```

#### **Explanation**:
- **Comprehensive CRUD Tests**: The tests validate the full range of operations for the `PackingListRepository`, ensuring that all interactions with the database work correctly.
- **Item Management**: The tests include specific checks for adding, updating, and removing `Item` entities from the `PackingList`, ensuring correct aggregate behavior.
- **Manual Debugging**: As with the `UserRepository`, the `PauseTest` method can be used to pause the tests and inspect the state of the database manually.

---

### 7. Running the Tests Conditionally

To run these tests conditionally, you need to set the environment variable `USE_REAL_DATABASE`. If this variable is not set, the tests will be skipped.

```bash
export USE_REAL_DATABASE=true
dotnet test tests/PackAndGo.Infrastructure.IntegrationTests.RealDb
```

To enable manual pauses between tests for debugging purposes, set the `ENABLE_MANUAL_TEST_PAUSE` environment variable:

```bash
export ENABLE_MANUAL_TEST_PAUSE=true
```

This will pause each test and allow you to inspect the database state.

---

### 8. Summary

In this chapter, we have demonstrated how to perform integration tests in the infrastructure layer using a real SQLite database. By leveraging conditional test execution and manual pauses for debugging, we ensure that our tests are thorough and easy to inspect. We've covered:
- **Setting up an SQLite database for testing** using a `DatabaseFixture`.
- **Writing integration tests** for both `UserRepository` and `PackingListRepository`.
- **Controlling test execution** using environment variables to conditionally run tests and enable manual inspection.

This approach ensures that your infrastructure components are well-tested in a real database environment while maintaining flexibility in how and when the tests are run.