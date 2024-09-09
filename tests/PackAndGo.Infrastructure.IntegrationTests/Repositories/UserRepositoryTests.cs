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
