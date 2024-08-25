## Improve the Domain Layer with DDD Principles

### Introduction

In the previous chapter, we built a minimalistic but functional web application using Clean Architecture principles. We created a basic `User` entity, defined repositories, services, and controllers, and integrated Entity Framework Core with SQLite to persist data. Now, it's time to refine our domain model, making it more robust, expressive, and aligned with Domain-Driven Design (DDD) principles.

This chapter will focus on enhancing the domain layer while keeping the `User` entity lean. We'll explore the use of value objects, validate inputs, and address the challenges of rehydrating entities from the database.

By the end of this chapter, you will have a deeper understanding of how to design a robust domain model and how to integrate it into your application.

### Create a Base Class for Entities

A common pattern in DDD is to create a base class for entities. This base class typically handles common concerns like equality, identity, and possibly some basic domain events handling. By centralizing this logic, we reduce duplication and make our entities more consistent.

**Base Entity Class**

Let's create a new file named `Entity.cs` in a directory called `Common` in the `PackAndGo.Domain` project:

```csharp
namespace PackAndGo.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode() ^ 31; // XOR for better distribution
    }
}
```

**Explanation**

- **Identity**: The `Id` property is central to identifying each entity. By overriding `Equals` and `GetHashCode`, we ensure that two entities are considered equal if they share the same identity.
- **Equality**: This implementation standardizes how equality is handled across all entities in the domain.

### Create a Base Class for Value Objects

Value objects represent concepts in your domain that are defined by their attributes rather than a unique identity. Common examples include Money, DateRange, and Email. By creating a base class for value objects, we can standardize how equality is handled across all value objects.

**Value Object Base Class**

Let's create a new file named `ValueObject.cs` in the `Common` directory.

```csharp
namespace PackAndGo.Domain.Common;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType()) return false;

        var valueObject = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents().Aggregate(1, (current, obj) => current * 23 + (obj?.GetHashCode() ?? 0));
    }
}
```

**Explanation**

- **Equality**: Value objects are considered equal if all their attributes are equal. The GetEqualityComponents method, implemented by subclasses, provides the components that define equality.
- **Immutability**: Value objects should be immutable. Once created, their state should not change.

### Implement Email as Value Object

Thus far, we’ve used a `string` to represent the email in the `User` entity. While this is simple, it has limitations. For example, it spreads validation logic across the codebase, and the email is treated as just another string. By introducing a value object, we can encapsulate the email logic and ensure consistency across the application.

**Email as a Value Object**

Now, we can refactor the `Email` into a value object. Let's create a new file named `Email.cs` in a new directory called `ValueObjects`.

```csharp
using System.Text.RegularExpressions;
using PackAndGo.Domain.Common;

namespace PackAndGo.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Email address is invalid", nameof(email));

        Value = email;
    }

    private static bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailRegex);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

By converting `Email` into a value object, we encapsulate all validation and behavior associated with an email address. This not only makes the domain model more expressive but also ensures that the same rules are applied consistently across the entire application.


### Refactor the User Entity

Let us now refactor the User Entity to become more robust.

**Refactoring the `User` Entity**

1. Let the User class inherit from our new base class `Entity` and remove the `Id` property, which is now defined in the base class.
2. Change the type for Email to our new Email value object
3. Make the constructor private to enforce the use of the `Create` method, which controls the instantiation of `User`.
4. Add a _static factory method_ `Create` that accepts a _string_ for the email. The Create method will also create a new Guid
5. Add a ChangeEmail method in oder to change the encapsulated Email property according to _Behavior-Driven Design_
6. We also add a `Load` method in order to recreate the entity from the database - More about that later.


```csharp
using PackAndGo.Domain.Common;
using PackAndGo.Domain.ValueObjects;

namespace PackAndGo.Domain.Entities;

public class User : Entity
{
    public Email Email { get; private set; }

    private User(Guid id, Email email)
    {
        Id = id;
        Email = email;
    }

    public static User Create(string email)
    {
        return new User(Guid.NewGuid(), new Email(email));
    }

    public static User Load(Guid id, string email)
    {
        return new User(id, new Email(email));
    }

    public void ChangeEmail(string email)
    {
        Email = new Email(email);
    }
}
```

#### Explanation

- **Static factory methods** are a powerful pattern for creating instances of entities. They provide a more expressive and controlled way of creating objects, especially when certain invariants or complex creation logic needs to be enforced.
- **Value Objects**: The `User` entity now directly uses the `Email` value object, ensuring that all email-related logic is encapsulated within the `Email` class.
- **Behavior-Driven Design**: Encapsulating domain logic within entities themselves is a core principle of Behavior-Driven Design. This ensures that our entities are not just data holders but are also responsible for enforcing business rules. By placing the ChangeEmail method within the User entity, we ensure that any rules or side effects associated with changing an email are handled internally, reducing the risk of errors and inconsistencies.
- **Load(Guid id, string email)**: Note how we need to have a "re-hydration" `Load` method in the User in order to be able to recreate the user from the database.


### Changes in Other Layers Due To Refactoring

To accommodate these changes in the domain layer, we need to make some adjustments in the application and infrastructure layers.

#### Application Layer

In the `UserService`, update how the `User` entity is instantiated and how you get the value from the Email value object.

Also note how we can´t create a new User as we did before in order to update the email address. Instead we need to retreive the User and then use the ChangeEmail method in order accomplish the update.

```csharp
using PackAndGo.Application.DTOs;
using PackAndGo.Application.Interfaces;
using PackAndGo.Domain.Entities;
using PackAndGo.Domain.Repositories;

namespace PackAndGo.Application.Services;

public class UserService : IUserService
{
   private readonly IUserRepository _userRepository;

   public UserService(IUserRepository userRepository)
   {
       _userRepository = userRepository;
   }

   public async Task<UserDTO?> GetUserByIdAsync(Guid id)
   {
       var user = await _userRepository.GetByIdAsync(id);
       return user == null ? null : new UserDTO { Id = user.Id, Email = user.Email.Value };
   }

   public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
   {
       return (await _userRepository.GetAllAsync())
           .Select(user => new UserDTO { Id = user.Id, Email = user.Email.Value });
   }

   public async Task AddUserAsync(UserDTO userDto)
   {
       var user = User.Create(userDto.Email);
       await _userRepository.AddAsync(user);
   }

   public async Task UpdateUserAsync(UserDTO userDto)
   {
        // Find the user by id
         var user = await _userRepository.GetByIdAsync(userDto.Id) ?? throw new Exception("User not found");
         
        // Update the user
        user.ChangeEmail(userDto.Email);
        await _userRepository.UpdateAsync(user);
    }

   public async Task DeleteUserAsync(Guid id)
   {
       await _userRepository.DeleteAsync(id);
   }
}
```

#### Infrastructure Layer

One challenge that arises when using value objects and encapsulating logic within entities is how to rehydrate these entities from the database. Rehydration is a term often used to describe the re-creation of an entity from the database. When an ORM like Entity Framework loads entities from the database, it typically bypasses constructors, which can make it difficult to enforce invariants and encapsulate logic. We have previously used a UserDataModel to abstract this away. 

We will explore both ways to deal with rehydration of domain entities from the database - directly in DbContext by the ORM and like before using a DataModel

Another approach is to use a separate data model class to abstract the database representation from the domain model. This way, the `UserDataModel` handles rehydration and database interactions, while the domain model remains clean and focused on business logic.

**UserDataModel**

Previously we used a separate data model class to abstract the database representation from the domain model. This way, the `UserDataModel` handles rehydration and database interactions, while the domain model remains clean and focused on business logic.

Let us adjust the `UserDataModel` in order to accomodate the Email value object used in the `User` domain entity.

```csharp
using PackAndGo.Domain.Entities;

namespace PackAndGo.Infrastructure.DataModels;

public class UserDataModel
{
   public Guid Id { get; set; }
   public string Email { get; set; } = string.Empty;

   public User ToDomain()
   {
      return User.Load(Id, Email);
   }

   public static UserDataModel FromDomain(User user)
   {
      return new UserDataModel
      {
         Id = user.Id,
         Email = user.Email.Value
      };
   }

}
```

**Explanation**

- **ToDomain**: Converts a `UserDataModel` into a domain `User` entity, ensuring that the proper creation logic is applied.
- **FromDomain**: Converts a `User` entity into a `UserDataModel`, preparing it for persistence.
- **User.Load(Id, Email)**: Note how we need to have a "re-hydration" `Load` method in the User in order to be able to recreate the user from the database.

We also need to adjust the the `UserRepository` to reflect the changes made in the User entity and the UserDataModel:

```csharp
using Microsoft.EntityFrameworkCore;
using PackAndGo.Domain.Entities;
using PackAndGo.Domain.Repositories;
using PackAndGo.Infrastructure.DataModels;
using PackAndGo.Infrastructure.Persistence;

namespace PackAndGo.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
   private readonly AppDbContext _context;

   public UserRepository(AppDbContext context)
   {
       _context = context;
   }

   public async Task<User?> GetByIdAsync(Guid id)
   {
       var dataModel = await _context.Users.FindAsync(id);
       return dataModel == null ? null : dataModel.ToDomain();
   }

   public async Task<IEnumerable<User>> GetAllAsync()
   {
        return await _context.Users
            .Select(u => u.ToDomain())
            .ToListAsync();
   }

   public async Task AddAsync(User user)
   {
       var dataModel = UserDataModel.FromDomain(user);
       _context.Users.Add(dataModel);
       await _context.SaveChangesAsync();
   }

   public async Task UpdateAsync(User user)
   {
        var dataModel = await _context.Users.FindAsync(user.Id);
        if (dataModel != null)
        {
            dataModel.Email = user.Email.Value;
            await _context.SaveChangesAsync();
        }
   }

   public async Task DeleteAsync(Guid id)
   {
       var dataModel = await _context.Users.FindAsync(id);
       if (dataModel != null)
       {
           _context.Users.Remove(dataModel);
           await _context.SaveChangesAsync();
       }
   }
}
```

##### Alternative Implementation With Reflection in the DbContext

TODO

#### Web Layer

No changes are needed, as the adjustments in the domain and application layers will be reflected automatically.


### Summary

In this chapter, we enhanced our domain model by incorporating value objects, implementing static factory methods with validation, and addressing the challenges of rehydration from the database. The key learning outcomes include:

1. **Entity Base Class**: We standardized identity and equality logic across all entities using a base class.
2. **Static Factory Method**: We introduced a static factory method for creating `User` entities, which ensures that essential business rules are enforced at the point of creation.
3. **Behavior-Driven Design**: We encapsulated domain logic, such as changing an email, within the entity itself, reinforcing the principle that entities should manage their own behavior.
4. **Value Object Implementation**: We refactored the `Email` class into a value object, making our domain model more expressive and consistent.
5. **Challenges of Rehydration**: We identified the potential issues with rehydrating entities from the database and explored solutions, including reflection and using a data model abstraction.

By completing this chapter, you’ve improved the robustness and expressiveness of your domain layer and learned how to address common challenges in a clean, maintainable way.