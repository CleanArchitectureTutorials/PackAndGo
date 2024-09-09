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
