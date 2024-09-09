using PackAndGo.Domain.Exceptions;

namespace PackAndGo.Domain.UnitTests.Exceptions;

public class InvalidEmailExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetCorrectErrorMessage()
    {
        // Arrange
        var invalidEmail = "invalid-email";

        // Act
        var exception = new InvalidEmailException(invalidEmail);

        // Assert
        Assert.Equal($"The email '{invalidEmail}' is not valid.", exception.Message);
    }
}
