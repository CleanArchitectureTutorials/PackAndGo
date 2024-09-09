
### Chapter: Infrastructure Integration Testing

In this chapter, we will explore how to conduct integration testing for the infrastructure layer of your application. Integration testing ensures that components like repositories, which interact with external systems (e.g., databases), work correctly as a unit. We will use an in-memory database to simulate a real database, providing a lightweight and fast environment for testing.

We’ll set up integration tests for the `UserRepository` and `PackingListRepository` in a dedicated test project, using a test fixture to handle common setup and teardown logic.

### **1. Setting Up the Integration Test Project**

Before we write any tests, we need to set up a new test project specifically for integration tests.

#### **1.1 Create the Integration Test Project**

Run the following command from the root of your solution to create a new `xUnit` test project:

```bash
dotnet new xunit -o tests/PackAndGo.Infrastructure.IntegrationTests

dotnet add tests/PackAndGo.Infrastructure.IntegrationTests reference src/PackAndGo.Infrastructure
dotnet add tests/PackAndGo.Infrastructure.IntegrationTests reference src/PackAndGo.Domain

dotnet sln add tests/PackAndGo.Infrastructure.IntegrationTests/PackAndGo.Infrastructure.IntegrationTests.csproj
```

This command creates a new `xUnit` test project in the `tests/PackAndGo.Infrastructure.IntegrationTests` directory.

#### **1.2 Add Necessary Packages**

Next, we need to add the necessary NuGet packages for EF Core’s in-memory database provider, as well as references to the infrastructure and domain layers:

```bash
dotnet add tests/PackAndGo.Infrastructure.IntegrationTests package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/PackAndGo.Infrastructure.IntegrationTests package Microsoft.EntityFrameworkCore

```

### **2. Directory Structure**

Your solution should now have the following structure:

```plaintext
PackAndGo/
├── PackAndGo.sln
├── src/
│   ├── PackAndGo.Domain/
│   │   └── ...
│   ├── PackAndGo.Infrastructure/
│   │   └── ...
├── tests/
│   └── PackAndGo.Infrastructure.IntegrationTests/
│       ├── Fixtures/
│       ├── Repositories/
│       └── ...
└── ...
```

- **Fixtures**: Contains the test fixture class that sets up the in-memory database and `AppDbContext`.
- **Repositories**: Contains the integration tests for the repository classes.

### **3. Implementing the Test Fixture**

A test fixture allows you to share setup and teardown code across multiple tests, ensuring a consistent test environment. In this case, the fixture will set up an in-memory database using EF Core.

#### **3.1 Creating the Test Fixture**

Create a file named `DatabaseFixture.cs` in the `Fixtures` directory:

```csharp
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
```

### **4. Writing Integration Tests for Repositories**

Now that the fixture is in place, we can write integration tests for the `UserRepository` and `PackingListRepository`.

#### **4.1 Integration Test for `UserRepository`**

Create a file named `UserRepositoryTests.cs` in the `Repositories` directory:

```csharp
using PackAndGo.Domain.Entities;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.IntegrationTests.Fixtures;

namespace PackAndGo.Infrastructure.IntegrationTests.Repositories;

public class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly UserRepository _repository;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new UserRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = User.Create("user@example.com");

        // Act
        await _repository.AddAsync(user);
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user@example.com", result.Email.Value);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = User.Create("user1@example.com");
        var user2 = User.Create("user2@example.com");
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);

        // Act
        var users = await _repository.GetAllAsync();

        // Assert at least 2 users
        Assert.NotNull(users);
        Assert.True(users.Count() >= 2); // There coold be more users due to seeding
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUserEmail()
    {
        // Arrange
        var user = User.Create("user@example.com");
        await _repository.AddAsync(user);

        // Act
        user.ChangeEmail("updateduser@example.com");
        await _repository.UpdateAsync(user);
        var updatedUser = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(updatedUser);
        Assert.Equal("updateduser@example.com", updatedUser.Email.Value);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var user = User.Create("user@example.com");
        await _repository.AddAsync(user);

        // Act
        await _repository.DeleteAsync(user.Id);
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.Null(result);
    }
}
```

#### **4.2 Integration Test for `PackingListRepository`**

Create a file named `PackingListRepositoryTests.cs` in the `Repositories` directory:

```csharp
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.IntegrationTests.Fixtures;

namespace PackAndGo.Infrastructure.IntegrationTests.Repositories
{
    public class PackingListRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly PackingListRepository _repository;

        public PackingListRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _repository = new PackingListRepository(_fixture.Context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPackingListToDatabase()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Toothbrush");

            // Act
            await _repository.AddAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Vacation", result.Name);
            Assert.Single(result.Items);
            Assert.Equal("Toothbrush", result.Items.First().Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPackingListWithItems()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Toothbrush");

            await _repository.AddAsync(packingList);

            // Act
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Vacation", result.Name);
            Assert.Single(result.Items);
            Assert.Equal("Toothbrush", result.Items.First().Name);
        }

        [Fact]
        public async Task GetAllByOwnerIdAsync_ShouldReturnAllPackingListsForOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var packingList1 = PackingList.Create("Vacation", ownerId);
            var packingList2 = PackingList.Create("Business Trip", ownerId);
            await _repository.AddAsync(packingList1);
            await _repository.AddAsync(packingList2);

            // Act
            var results = await _repository.GetAllByOwnerIdAsync(ownerId);

            // Assert
            Assert.Equal(2, results.Count());
            Assert.Contains(results, pl => pl.Name == "Vacation");
            Assert.Contains(results, pl => pl.Name == "Business Trip");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdatePackingListName()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            await _repository.AddAsync(packingList);

            // Act
            packingList.ChangeName("Updated Vacation");
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Vacation", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAddNewItemToPackingList()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            await _repository.AddAsync(packingList);

            // Act
            packingList.AddItem("Sunglasses");
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Contains(result.Items, i => i.Name == "Sunglasses");
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldUpdateExistingItem()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Sunglasses");
            await _repository.AddAsync(packingList);
            var itemId = packingList.Items.First().Id;

            // Act
            packingList.ChangeItemName(itemId, "Updated Sunglasses");
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Updated Sunglasses", result.Items.First().Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldRemoveItemFromPackingList()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Sunglasses");
            await _repository.AddAsync(packingList);

            // Act
            var itemId = packingList.Items.First().Id;
            packingList.RemoveItem(itemId);
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task UpdateAsync_ShouldHandleMultipleChanges()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            packingList.AddItem("Sunglasses");
            packingList.AddItem("Hat");
            await _repository.AddAsync(packingList);

            // Act
            var sunglassesId = packingList.Items.First(i => i.Name == "Sunglasses").Id;
            packingList.ChangeItemName(sunglassesId, "Updated Sunglasses");
            packingList.RemoveItem(sunglassesId);
            packingList.AddItem("Shoes");
            await _repository.UpdateAsync(packingList);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.DoesNotContain(result.Items, i => i.Name == "Sunglasses");
            Assert.Contains(result.Items, i => i.Name == "Hat");
            Assert.Contains(result.Items, i => i.Name == "Shoes");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemovePackingListFromDatabase()
        {
            // Arrange
            var packingList = PackingList.Create("Vacation", Guid.NewGuid());
            await _repository.AddAsync(packingList);

            // Act
            await _repository.DeleteAsync(packingList.Id);
            var result = await _repository.GetByIdAsync(packingList.Id);

            // Assert
            Assert.Null(result);
        }
    }
}
```

### **5. Running the Tests**

Once the integration tests are implemented, you can run them using the following command from the root of your solution:

```bash
dotnet test tests/PackAndGo.Infrastructure.IntegrationTests
```

This command will build your test project, execute the tests, and display the results.

### **6. Summary**

In this chapter, we've explored how to set up and implement integration tests for the infrastructure layer of your application. By using an in-memory database and a test fixture, we've created a reliable testing environment that ensures your repositories interact correctly with the database.

### **Learning Outcomes**

By the end of this chapter, you should be able to:

1. Set up an `xUnit` integration test project for the infrastructure layer.
2. Use a test fixture to manage the setup and teardown of an in-memory database.
3. Implement integration tests for repositories, ensuring they correctly perform CRUD operations in the database.
4. Run and analyze integration tests to verify the correctness of your infrastructure code.

These skills are essential for ensuring that your data access layer is robust, reliable, and behaves as expected when interacting with the database.