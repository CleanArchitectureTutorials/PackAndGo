
### Unit Testing Data Models in the Infrastructure Layer

In this chapter, we will explore how to effectively unit test data models in the infrastructure layer of your application. These data models are crucial as they serve as the bridge between your domain entities and the underlying database. We'll walk through the setup, implementation, and execution of tests for these data models, ensuring that they function correctly within the context of your application. We'll also cover how to configure your testing environment using EF Core's in-memory database.

### **1. Setting Up the Test Project**

Before we begin testing the data models, we need to ensure that our test environment is properly set up.

#### **1.1 Create the Test Project**

First, create a new test project in your solution. Navigate to the root of your solution and run the following command:

```bash
dotnet new xunit -o tests/PackAndGo.Infrastructure.UnitTests

dotnet add tests/PackAndGo.Infrastructure.UnitTests reference src/PackAndGo.Infrastructure
dotnet add tests/PackAndGo.Infrastructure.UnitTests reference src/PackAndGo.Domain
```

This command creates a new `xUnit` test project in the `tests/PackAndGo.Infrastructure.UnitTests` directory.

#### **1.2 Add Necessary Packages**

Next, we need to add the necessary NuGet packages to the test project, including EF Core's in-memory database provider and a reference to the infrastructure project.

Run the following commands:

```bash
dotnet add tests/PackAndGo.Infrastructure.UnitTests package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/PackAndGo.Infrastructure.UnitTests package Microsoft.EntityFrameworkCore

```

These commands add the required packages for EF Core and ensure that the test project can access the infrastructure and domain layers.

#### **1.3 Add the Test Project to the Solution**

Ensure that the test project is part of your solution:

```bash
dotnet sln add tests/PackAndGo.Infrastructure.UnitTests/PackAndGo.Infrastructure.UnitTests.csproj
```

### **2. Directory Structure**

After setting up the test project, your directory structure should look like this:

```plaintext
PackAndGo/
├── PackAndGo.sln
├── src/
│   ├── PackAndGo.Domain/
│   │   └── ...
│   ├── PackAndGo.Infrastructure/
│   │   └── ...
├── tests/
│   └── PackAndGo.Infrastructure.UnitTests/
│       ├── DataModels/
│       └── ...
└── ...
```

- **DataModels**: Contains unit tests for the data models in the infrastructure layer.

### **3. Implementing Unit Tests for Data Models**

Now that our test project is set up, let's write unit tests for each of the data models: `UserDataModel`, `ItemDataModel`, and `PackingListDataModel`.

#### **3.1 Unit Test for `UserDataModel`**

Create a file named `UserDataModelTests.cs` in the `DataModels` directory:

```csharp
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
```

#### **3.2 Unit Test for `ItemDataModel`**

Create a file named `ItemDataModelTests.cs` in the `DataModels` directory:

```csharp
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.UnitTests.DataModels;

public class ItemDataModelTests
{
    [Fact]
    public void ToDomain_ShouldReturnValidItem()
    {
        // Arrange
        var itemDataModel = new ItemDataModel
        {
            Id = Guid.NewGuid(),
            Name = "Toothbrush",
            IsPacked = false
        };

        // Act
        var item = itemDataModel.ToDomain();

        // Assert
        Assert.NotNull(item);
        Assert.Equal(itemDataModel.Id, item.Id);
        Assert.Equal(itemDataModel.Name, item.Name);
        Assert.Equal(itemDataModel.IsPacked, item.IsPacked);
    }

    [Fact]
    public void FromDomain_ShouldReturnValidItemDataModel()
    {
        // Arrange
        var item = Item.Create("Toothbrush");

        // Act
        var itemDataModel = ItemDataModel.FromDomain(item);

        // Assert
        Assert.NotNull(itemDataModel);
        Assert.Equal(item.Id, itemDataModel.Id);
        Assert.Equal(item.Name, itemDataModel.Name);
        Assert.Equal(item.IsPacked, itemDataModel.IsPacked);
    }
}
```

#### **3.3 Unit Test for `PackingListDataModel`**

Create a file named `PackingListDataModelTests.cs` in the `DataModels` directory:

```csharp
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.UnitTests.DataModels;

public class PackingListDataModelTests
{
    [Fact]
    public void ToDomain_ShouldReturnValidPackingList()
    {
        // Arrange
        var packingListDataModel = new PackingListDataModel
        {
            Id = Guid.NewGuid(),
            Name = "Vacation",
            UserId = Guid.NewGuid(),
            Items = new List<ItemDataModel>
            {
                new ItemDataModel { Id = Guid.NewGuid(), Name = "Toothbrush", IsPacked = false }
            }
        };

        // Act
        var packingList = packingListDataModel.ToDomain();

        // Assert
        Assert.NotNull(packingList);
        Assert.Equal(packingListDataModel.Id, packingList.Id);
        Assert.Equal(packingListDataModel.Name, packingList.Name);
        Assert.Equal(packingListDataModel.UserId, packingList.OwnerId);
        Assert.Single(packingList.Items);
    }

    [Fact]
    public void FromDomain_ShouldReturnValidPackingListDataModel()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        packingList.AddItem("Toothbrush");

        // Act
        var packingListDataModel = PackingListDataModel.FromDomain(packingList);

        // Assert
        Assert.NotNull(packingListDataModel);
        Assert.Equal(packingList.Id, packingListDataModel.Id);
        Assert.Equal(packingList.Name, packingListDataModel.Name);
        Assert.Equal(packingList.OwnerId, packingListDataModel.UserId);
        Assert.NotNull(packingListDataModel.Items);
        Assert.Single(packingListDataModel.Items);
    }
}
```

### **4. Running the Tests**

Once the tests are implemented, you can run them using the following command from the root of your solution:

```bash
dotnet test tests/PackAndGo.Infrastructure.UnitTests
```

This command will build your test project, execute the tests, and report the results.

### **5. Summary**

In this chapter, we've walked through setting up and implementing unit tests for the data models in the infrastructure layer. These tests ensure that the data models correctly map to and from the domain entities, preserving the integrity of the data as it moves between the application layers.

### **Learning Outcomes**

By the end of this chapter, you should be able to:

1. Set up an `xUnit` test project for the infrastructure layer.
2. Implement unit tests for data models, ensuring they correctly map to and from domain entities.
3. Run tests to verify the accuracy and reliability of your data models in the infrastructure layer.

This testing approach helps maintain the consistency and correctness of the data as it flows through your application, particularly when interacting with the database.