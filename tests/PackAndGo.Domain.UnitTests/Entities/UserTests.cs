using PackAndGo.Domain.Entities;
using PackAndGo.Domain.Exceptions;

namespace PackAndGo.Domain.UnitTests.Entities;

public class UserTests
{
    [Fact]
    public void Create_ShouldReturnValidUser()
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var user = User.Create(email);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(email, user.Email.Value);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Create_ShouldThrowInvalidEmailException_WhenEmailIsInvalid()
    {
        // Arrange
        var invalidEmail = "invalid-email";

        // Act & Assert
        Assert.Throws<InvalidEmailException>(() => User.Create(invalidEmail));
    }

    [Fact]
    public void Create_ShouldThrowEmptyEmailException_WhenEmailIsNull()
    {
        // Arrange
        string nullEmail = null!;

        // Act & Assert
        Assert.Throws<EmptyEmailException>(() => User.Create(nullEmail));
    }

    [Fact]
    public void Load_ShouldReturnUserWithGivenProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var email = "user@example.com";

        // Act
        var user = User.Load(id, email);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(id, user.Id);
        Assert.Equal(email, user.Email.Value);
    }

    [Fact]
    public void ChangeEmail_ShouldUpdateEmail()
    {
        // Arrange
        var user = User.Create("old@example.com");
        var newEmail = "new@example.com";

        // Act
        user.ChangeEmail(newEmail);

        // Assert
        Assert.Equal(newEmail, user.Email.Value);
    }

    [Fact]
    public void ChangeEmail_ShouldThrowInvalidEmailException_WhenNewEmailIsInvalid()
    {
        // Arrange
        var user = User.Create("user@example.com");
        var invalidEmail = "invalid-email";

        // Act & Assert
        Assert.Throws<InvalidEmailException>(() => user.ChangeEmail(invalidEmail));
    }

    [Fact]
    public void ChangeEmail_ShouldThrowEmptyEmailException_WhenNewEmailIsNull()
    {
        // Arrange
        var user = User.Create("user@example.com");

        // Act & Assert
        Assert.Throws<EmptyEmailException>(() => user.ChangeEmail(null!));
    }

    // Test the base class Entity

    [Fact]
    public void Users_ShouldBeEqual_WhenIdsAreIdentical()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user1 = User.Load(id, "user@example.com");
        var user2 = User.Load(id, "other@example.com");

        // Act & Assert
        Assert.Equal(user1, user2);
    }

    [Fact]
    public void Users_ShouldNotBeEqual_WhenIdsAreDifferent()
    {
        // Arrange
        var user1 = User.Create("user@example.com");
        var user2 = User.Create("user@example.com");

        // Act & Assert
        Assert.NotEqual(user1, user2);
    }

    [Fact]
    public void GetHashCode_ShouldBeSameForUsersWithSameId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user1 = User.Load(id, "user@example.com");
        var user2 = User.Load(id, "other@example.com");

        // Act
        var hashCode1 = user1.GetHashCode();
        var hashCode2 = user2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferentForUsersWithDifferentIds()
    {
        // Arrange
        var user1 = User.Create("user@example.com");
        var user2 = User.Create("other@example.com");

        // Act
        var hashCode1 = user1.GetHashCode();
        var hashCode2 = user2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithNull()
    {
        // Arrange
        var user = User.Create("user@example.com");

        // Act
        var result = user.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithDifferentType()
    {
        // Arrange
        var user = User.Create("user@example.com");
        var otherObject = "some-string"; // Just a string, not a User

        // Act
        var result = user.Equals(otherObject);

        // Assert
        Assert.False(result);
    }
}
