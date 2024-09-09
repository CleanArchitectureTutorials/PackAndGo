using PackAndGo.Domain.Exceptions;
using PackAndGo.Domain.ValueObjects;

namespace PackAndGo.Domain.UnitTests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Constructor_ShouldCreateEmail_WhenEmailIsValid()
    {
        // Arrange
        var validEmail = "user@example.com";

        // Act
        var email = new Email(validEmail);

        // Assert
        Assert.NotNull(email);
        Assert.Equal(validEmail, email.Value);
    }

    [Fact]
    public void Constructor_ShouldThrowEmptyEmailException_WhenEmailIsNullOrWhitespace()
    {
        // Arrange
        var emptyEmail = "";

        // Act & Assert
        Assert.Throws<EmptyEmailException>(() => new Email(emptyEmail));
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@missingusername.com")]
    [InlineData("username@.com")]
    [InlineData("username@domain")]
    // [InlineData("username@domain..com")]
    public void Constructor_ShouldThrowInvalidEmailException_WhenEmailIsInvalid(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<InvalidEmailException>(() => new Email(invalidEmail));
    }

    [Fact]
    public void Emails_ShouldBeEqual_WhenValuesAreIdentical()
    {
        // Arrange
        var email1 = new Email("user@example.com");
        var email2 = new Email("user@example.com");

        // Act & Assert
        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Emails_ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange
        var email1 = new Email("user@example.com");
        var email2 = new Email("other@example.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
    }

    // GetHashCode() and Equals() are overridden in the ValueObject base class
    // So we need to test them to make sure they work as expected
    // We can't test them directly in the ValueObject class because it's abstract

    [Fact]
    public void GetHashCode_ShouldBeSameForEqualEmails()
    {
        // Arrange
        var email1 = new Email("user@example.com");
        var email2 = new Email("user@example.com");

        // Act
        var hashCode1 = email1.GetHashCode();
        var hashCode2 = email2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferentForDifferentEmails()
    {
        // Arrange
        var email1 = new Email("user@example.com");
        var email2 = new Email("other@example.com");

        // Act
        var hashCode1 = email1.GetHashCode();
        var hashCode2 = email2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithNull()
    {
        // Arrange
        var email = new Email("user@example.com");

        // Act
        var result = email.Equals(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithDifferentType()
    {
        // Arrange
        var email = new Email("user@example.com");
        var otherObject = "user@example.com"; // Just a string, not an Email

        // Act
        var result = email.Equals(otherObject);

        // Assert
        Assert.False(result);
    }
}
