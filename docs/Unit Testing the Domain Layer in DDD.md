
### Unit Testing the Domain Layer in DDD

Unit testing is essential in Domain-Driven Design (DDD) to ensure the integrity of the business logic that resides in the domain layer. Testing the domain layer helps identify defects early, validate business rules, and ensure consistent domain behavior.

In this chapter, we’ll explore how to implement and structure unit tests for the domain model, specifically focusing on entities, value objects, and aggregates. We’ll cover the test setup, writing meaningful tests, and explaining how each test contributes to verifying the domain logic.

Unit testing in DDD ensures that the core business logic, encapsulated in domain entities, value objects, and aggregates, functions correctly. Since the domain layer represents the heart of the application, any errors here could lead to inconsistent or incorrect behavior throughout the system. Unit testing helps:
- Detect defects early.
- Validate complex business rules.
- Ensure the integrity of the domain model.
- Promote confidence when refactoring code.

During early development, unit testing helps ensure that the core domain logic is built correctly. By writing tests while developing the domain, you can:
- Validate that new features conform to business requirements.
- Ensure invariants and business rules are properly enforced.
- Prevent regressions when new functionality is introduced.

### Naming Conventions for Test Cases

Clear and consistent naming of unit tests is crucial for maintainability. Here are some general naming conventions used throughout this chapter:
- **MethodName_ShouldExpectedBehavior_WhenCondition**: This pattern helps make test cases self-explanatory. For instance, `Create_ShouldReturnValidUser_WhenEmailIsValid` describes that the `Create` method should return a valid user when the provided email is valid.
- **Use Assertions**: Tests include `Assert` statements that check whether the actual output meets the expected results.

### Directory Structure

After setting up the test project, the directory structure will look like this:

```plaintext
PackAndGo/
│
├── src/
│   ├── PackAndGo.Domain/
│   │   └── Aggregates/
│   │       └── PackingListAggregate/
│   │           ├── Entities/
│   │           ├── Repositories/
│   │           └── ... 
│   └── ...
├── tests/
│   └── PackAndGo.Domain.UnitTests/
│       ├── Fixtures/
│       ├── Entities/
│       └── Aggregates/
└── ...
```

- **Fixtures**: Contains classes that setup and configure the test environment.
- **Entities**: Contains unit tests for individual entities like `Item`.
- **Aggregates**: Contains unit tests for aggregates like `PackingList`.

### 1. Setting Up the Test Project

To begin, we’ll set up a testing project using `xUnit` to test our domain model. Follow these commands to create a test project and reference the domain project.

#### **1.1 Create the Test Project**

```bash
dotnet new xunit -o tests/PackAndGo.Domain.UnitTests
dotnet sln add tests/PackAndGo.Domain.UnitTests/PackAndGo.Domain.UnitTests.csproj
dotnet add tests/PackAndGo.Domain.UnitTests reference src/PackAndGo.Domain
```

This sets up the testing environment and references the domain layer so that we can test its components.

### 2. Writing Unit Tests for the Domain Layer

The following sections provide the unit tests for various components of the domain layer, including the `User` entity, `Email` value object, `Item` entity, and `PackingList` aggregate. Each test is explained after its corresponding file to provide clarity on the testing strategy and purpose.

---

### 2.1. `UserTests.cs`

```csharp
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
```

#### **Explanation: UserTests.cs**

- **`Create_ShouldReturnValidUser`**: Ensures a valid `User` object is created when a proper email is provided.
- **`Create_ShouldThrowInvalidEmailException_WhenEmailIsInvalid`**: Tests that an invalid email format triggers the `InvalidEmailException`.
- **`Create_ShouldThrowEmptyEmailException_WhenEmailIsNull`**: Ensures that `null` or empty email input results in an `EmptyEmailException`.
- **`Load_ShouldReturnUserWithGivenProperties`**: Verifies that a `User` with specific properties is correctly loaded into the domain.
- **`ChangeEmail_ShouldUpdateEmail`**: Confirms that changing the email of a user is reflected properly.

---

### 2.2. `EmailTests.cs`

```csharp
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
```

#### **Explanation: EmailTests.cs**

- **`Constructor_ShouldCreateEmail_WhenEmailIsValid`**: Tests the creation of an `Email` object with a valid email string.
- **`Constructor_ShouldThrowEmptyEmailException_WhenEmailIsNullOrWhitespace`**: Ensures that empty or whitespace emails are invalid.
- **`Constructor_ShouldThrowInvalidEmailException_WhenEmailIsInvalid`**: Uses the `Theory` attribute to validate various invalid email formats, expecting exceptions for each case.
- **`Emails_ShouldBeEqual_WhenValuesAreIdentical`**: Verifies that two `Email` objects with identical values are considered equal.
- **`Emails_ShouldNotBeEqual_WhenValuesAreDifferent`**: Ensures that two `Email` objects with different values are not equal.

---
### 2.3. `EmptyEmailExceptionTests.cs`

```csharp
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
```

#### **Explanation: EmptyEmailExceptionTests.cs**
  
- **`EmptyEmailExceptionTests`**:
  - **`Constructor_ShouldSetCorrectErrorMessage`**: This test validates that when an `EmptyEmailException` is thrown, it has the appropriate default error message: `"The email address cannot be null or empty."` This ensures that the exception provides a clear reason for failure when empty email addresses are encountered.

---

### 2.3. `InvalidEmailExceptionTests.cs`

```csharp
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
```

#### **Explanation: InvalidEmailExceptionTests.cs**

- **`InvalidEmailExceptionTests`**: 
  - **`Constructor_ShouldSetCorrectErrorMessage`**: This test checks that when an `InvalidEmailException` is thrown, it carries the correct message with the invalid email provided during instantiation. It ensures that the exception is meaningful and contains the necessary context for debugging.
  
---

### 2.3. `ItemTests.cs`

```csharp
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Domain.UnitTests.Aggregates.PackingListAggregate.Entities;

public class ItemTests
{
    [Fact]
    public void Create_ShouldReturnValidItem()
    {
        // Arrange
        var itemName = "Toothbrush";

        // Act
        var item = Item.Create(itemName);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(itemName, item.Name);
        Assert.False(item.IsPacked);
        Assert.NotEqual(Guid.Empty, item.Id);
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Item.Create(null!));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Item.Create(string.Empty));
    }

    [Fact]
    public void Load_ShouldReturnItemWithGivenProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var itemName = "Shampoo";
        var isPacked = true;

        // Act
        var item = Item.Load(id, itemName, isPacked);

        // Assert
        Assert.NotNull(item);
        Assert.Equal(id, item.Id);
        Assert.Equal(itemName, item.Name);
        Assert.Equal(isPacked, item.IsPacked);
    }

    [Fact]
    public void ChangeName_ShouldUpdateName()
    {
        // Arrange
        var item = Item.Create("Toothbrush");
        var newName = "Shampoo";

        // Act
        item.ChangeName(newName);

        // Assert
        Assert.Equal(newName, item.Name);
    }

    [Fact]
    public void ChangeName_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        var item = Item.Create("Toothbrush");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => item.ChangeName(null!));
    }

    [Fact]
    public void ChangeName_ShouldThrowArgumentNullException_WhenNameIsEmpty()
    {
        // Arrange
        var item = Item.Create("Toothbrush");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => item.ChangeName(string.Empty));
    }

    [Fact]
    public void MarkAsPacked_ShouldSetIsPackedToTrue()
    {
        // Arrange
        var item = Item.Create("Toothbrush");

        // Act
        item.MarkAsPacked();

        // Assert
        Assert.True(item.IsPacked);
    }

    [Fact]
    public void MarkAsUnpacked_ShouldSetIsPackedToFalse()
    {
        // Arrange
        var item = Item.Create("Toothbrush");
        item.MarkAsPacked();

        // Act
        item.MarkAsUnpacked();

        // Assert
        Assert.False(item.IsPacked);
    }
}
```

#### **Explanation: ItemTests.cs**

- **`Create_ShouldReturnValidItem`**: Ensures a valid `Item` entity is created with the given name and defaults to unpacked.
- **`Create_ShouldThrowArgumentNullException_WhenNameIsNull`**: Tests that passing a `null` item name triggers an `ArgumentNullException`.
- **`Create_ShouldThrowArgumentNullException_WhenNameIsEmpty`**: Ensures that an empty string name also throws an `ArgumentNullException`.
- **`MarkAsPacked_ShouldSetIsPackedToTrue`**: Validates that calling `MarkAsPacked` sets the `IsPacked` property to `true`.
- **`MarkAsUnpacked_ShouldSetIsPackedToFalse`**: Ensures that calling `MarkAsUnpacked` after marking it packed sets the property back to `false`.

---

### 2.4. `PackingListTests.cs`

```csharp
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Domain.UnitTests.Aggregates.PackingListAggregate.Entities;

public class PackingListTests
{
    [Fact]
    public void Create_ShouldReturnValidPackingList()
    {
        // Arrange
        var packingListName = "Vacation";
        var ownerId = Guid.NewGuid();

        // Act
        var packingList = PackingList.Create(packingListName, ownerId);

        // Assert
        Assert.NotNull(packingList);
        Assert.Equal(packingListName, packingList.Name);
        Assert.Equal(ownerId, packingList.OwnerId);
        Assert.Empty(packingList.Items);
        Assert.NotEqual(Guid.Empty, packingList.Id);
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PackingList.Create(null!, ownerId));
    }

    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenOwnerIdIsEmpty()
    {
        // Arrange
        var packingListName = "Vacation";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => PackingList.Create(packingListName, Guid.Empty));
    }

    [Fact]
    public void Load_ShouldReturnPackingListWithGivenProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var packingListName = "Vacation";
        var ownerId = Guid.NewGuid();
        var items = new List<Item> { Item.Create("Toothbrush") };

        // Act
        var packingList = PackingList.Load(id, packingListName, ownerId, items);

        // Assert
        Assert.NotNull(packingList);
        Assert.Equal(id, packingList.Id);
        Assert.Equal(packingListName, packingList.Name);
        Assert.Equal(ownerId, packingList.OwnerId);
        Assert.Equal(items.Count, packingList.Items.Count);
        Assert.Contains(packingList.Items, i => i.Name == "Toothbrush");
    }

    [Fact]
    public void ChangeName_ShouldUpdatePackingListName()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var newName = "Business Trip";

        // Act
        packingList.ChangeName(newName);

        // Assert
        Assert.Equal(newName, packingList.Name);
    }

    [Fact]
    public void ChangeName_ShouldThrowArgumentNullException_WhenNameIsNull()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => packingList.ChangeName(null!));
    }

    [Fact]
    public void AddItem_ShouldAddNewItemToPackingList()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";

        // Act
        packingList.AddItem(itemName);

        // Assert
        Assert.Single(packingList.Items);
        Assert.Contains(packingList.Items, i => i.Name == itemName);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemFromPackingList()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";
        packingList.AddItem(itemName);
        var itemId = packingList.Items.First().Id;

        // Act
        packingList.RemoveItem(itemId);

        // Assert
        Assert.Empty(packingList.Items);
    }

    [Fact]
    public void ChangeItemName_ShouldUpdateItemName()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";
        packingList.AddItem(itemName);
        var itemId = packingList.Items.First().Id;
        var newItemName = "Hat";

        // Act
        packingList.ChangeItemName(itemId, newItemName);

        // Assert
        Assert.Contains(packingList.Items, i => i.Name == newItemName);
        Assert.DoesNotContain(packingList.Items, i => i.Name == itemName);
    }

    [Fact]
    public void MarkItemAsPacked_ShouldSetItemAsPacked()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";
        packingList.AddItem(itemName);
        var itemId = packingList.Items.First().Id;

        // Act
        packingList.MarkItemAsPacked(itemId);

        // Assert
        Assert.True(packingList.Items.First(i => i.Id == itemId).IsPacked);
    }

    [Fact]
    public void MarkItemAsUnpacked_ShouldSetItemAsUnpacked()
    {
        // Arrange
        var packingList = PackingList.Create("Vacation", Guid.NewGuid());
        var itemName = "Sunglasses";
        packingList.AddItem(itemName);
        var itemId = packingList.Items.First().Id;

        // Act
        packingList.MarkItemAsPacked(itemId); // First, mark it as packed
        packingList.MarkItemAsUnpacked(itemId); // Then, mark it as unpacked

        // Assert
        Assert.False(packingList.Items.First(i => i.Id == itemId).IsPacked);
    }
}
```

#### **Explanation: PackingListTests.cs**

- **`Create_ShouldReturnValidPackingList`**: Ensures that a valid `PackingList` is created with the provided name and owner, and starts with an empty items collection.
- **`AddItem_ShouldAddNewItemToPackingList`**: Verifies that an item is correctly added to the `PackingList`.
- **`RemoveItem_ShouldRemoveItemFromPackingList`**: Tests that an item can be removed from the `PackingList`.
- **`ChangeItemName_ShouldUpdateItemName`**: Ensures that an item's name can be changed within the `PackingList`.

---

### 3. Running the Tests

After writing the unit tests, run them with the following command to ensure everything works as expected:

```bash
dotnet test tests/PackAndGo.Domain.UnitTests
```

This will build and execute the tests, showing the results in the terminal.

---

### 4. Summary

In this chapter, you learned how to set up and write comprehensive unit tests for the domain layer of a Domain-Driven Design (DDD) application. Specifically, we:
- Tested entities such as `User` and `Item` for correct business logic behavior.
- Tested value objects like `Email` to ensure they uphold the rules governing their structure.
- Verified aggregates like `PackingList` to ensure they correctly manage internal collections and enforce domain rules.

By implementing these unit tests, you ensure that your core domain logic is reliable, correctly follows business rules, and is ready for further integration into other layers of your application.