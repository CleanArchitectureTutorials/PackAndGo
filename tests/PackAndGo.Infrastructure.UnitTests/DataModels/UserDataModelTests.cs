using PackAndGo.Domain.Entities;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.UnitTests.DataModels;

public class UserDataModelTests
{
    [Fact]
    public void ToDomain_ShouldReturnValidUser()
    {
        // Arrange
        var userDataModel = new UserDataModel
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com"
        };

        // Act
        var user = userDataModel.ToDomain();

        // Assert
        Assert.NotNull(user);
        Assert.Equal(userDataModel.Id, user.Id);
        Assert.Equal(userDataModel.Email, user.Email.Value);
    }

    [Fact]
    public void FromDomain_ShouldReturnValidUserDataModel()
    {
        // Arrange
        var user = User.Create("user@example.com");

        // Act
        var userDataModel = UserDataModel.FromDomain(user);

        // Assert
        Assert.NotNull(userDataModel);
        Assert.Equal(user.Id, userDataModel.Id);
        Assert.Equal(user.Email.Value, userDataModel.Email);
    }
}
