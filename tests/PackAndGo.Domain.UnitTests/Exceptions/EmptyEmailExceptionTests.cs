using PackAndGo.Domain.Exceptions;

namespace PackAndGo.Domain.UnitTests.Exceptions;

public class EmptyEmailExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetCorrectErrorMessage()
    {
        // Act
        var exception = new EmptyEmailException();

        // Assert
        Assert.Equal("The email address cannot be null or empty.", exception.Message);
    }
}
