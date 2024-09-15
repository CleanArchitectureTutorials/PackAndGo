## Implementing Authentication with ASP.NET Core Identity and Domain Separation

In this chapter, we will walk through the implementation of authentication using **ASP.NET Core Identity** while keeping the **domain logic** separate from authentication concerns. We will explore how to distinguish between the **Domain User** and **Application User** using Identity's `IdentityUser` class. Our aim is to ensure that the **domain layer** remains clean and independent of the authentication mechanisms, while the **infrastructure layer** handles all aspects of identity and security.

### Overview

**ASP.NET Core Identity** provides a robust authentication system that includes features like login, registration, password management, and user roles. By using Identity’s `IdentityUser` class, we can manage authentication and user credentials in the **infrastructure layer**, while keeping our **domain user** logic separate and clean.

The architectural goals are:
- **Separation of Concerns**: Authentication logic remains in the **infrastructure layer**, while domain logic (user business rules) remains in the **domain layer**.
- **Transaction Management**: We use a **Unit of Work** pattern to ensure operations involving both the `IdentityUser` and `DomainUser` are executed in a single transaction.
- **Clear Data Model Separation**: We maintain distinct tables for **IdentityUser** (authentication) and **DomainUser** (business-specific user information).

### Setting Up the DbContext

We extend the `DbContext` to include both **IdentityUser** (for authentication) and **DomainUser** (for domain-specific user logic) by inheriting from **`IdentityDbContext<IdentityUser>`**. We rename the `Users` table used by the domain to avoid conflicts with the table used by Identity.

> `AppDbContext.cs`

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        // Domain-specific user table, separate from IdentityUser
        public DbSet<UserDataModel> DomainUsers { get; set; } 
        public DbSet<PackingListDataModel> PackingLists { get; set; }
        public DbSet<ItemDataModel> Items { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ensures Identity tables like AspNetUsers, AspNetRoles, etc. are configured properly
            base.OnModelCreating(modelBuilder);

            // Renaming Domain User table to avoid conflict with Identity's AspNetUsers
            modelBuilder.Entity<UserDataModel>().ToTable("DomainUsers");

            // Remove seeding from the DbContext to prevent conflicts during migrations
            //modelBuilder.Entity<UserDataModel>().HasData(
            //    new UserDataModel { Id = Guid.NewGuid(), Email = "john.doe@test.com" },
            //    new UserDataModel { Id = Guid.NewGuid(), Email = "jane.doe@test.com" },
            //    new UserDataModel { Id = Guid.NewGuid(), Email = "jacob.doe@test.com" }
            //);
        }
    }
}
```

**Explanation**:

- **IdentityUser**: This represents the authentication layer, and **ASP.NET Core Identity** will use this class to manage user credentials, roles, and claims. The `AppDbContext` inherits from `IdentityDbContext<IdentityUser>` to handle authentication.
- **DomainUsers**: We create a separate `DbSet<UserDataModel>` to store the domain-specific user data in the `DomainUsers` table. This table will store business-related user data, separate from the `AspNetUsers` table that handles authentication.
- **OnModelCreating**: The `base.OnModelCreating(modelBuilder)` is critical as it configures Identity's default tables (such as `AspNetUsers`, `AspNetRoles`). We also configure the `DomainUsers` table to avoid conflicts with the table used by **IdentityUser**.

By renaming the `Users` table to `DomainUsers`, we ensure that both authentication and domain-specific user data are handled separately, while maintaining clean architecture principles.

#### Remove seeding

When removing Seeding we need to remove it from the DbContext as above. We also need to stop it in the migrations, both the current and the previous like this:

```csharp
// migrationBuilder.DeleteData(

// table: "Users",

// keyColumn: "Id",

// keyValue: new Guid("097498c7-66ad-4a1f-b1a4-0843e89a78e2"));

  

// migrationBuilder.DeleteData(

// table: "Users",

// keyColumn: "Id",

// keyValue: new Guid("0c0e1af3-d9aa-4b86-a41b-fb44cdb685a4"));

  

// migrationBuilder.DeleteData(

// table: "Users",

// keyColumn: "Id",

// keyValue: new Guid("5458a1ce-d74f-42d8-b83d-b917cc21a8f9"));
```


```csharp
// migrationBuilder.InsertData(

// table: "Users",

// columns: new[] { "Id", "Email" },

// values: new object[,]

// {

// { new Guid("097498c7-66ad-4a1f-b1a4-0843e89a78e2"), "john.doe@test.com" },

// { new Guid("0c0e1af3-d9aa-4b86-a41b-fb44cdb685a4"), "jane.doe@test.com" },

// { new Guid("5458a1ce-d74f-42d8-b83d-b917cc21a8f9"), "jacob.doe@test.com" }

// });
```

#### Adding ASP.NET Core Identity Package

To integrate **ASP.NET Identity**, you need to add the necessary NuGet packages to your project. The command below adds the **ASP.NET Core Identity** package to your project.

```bash
dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

#### Migrate and update the Database

```bash
dotnet ef migrations add AddIdentityUser --project src/PackAndGo.Infrastructure --startup-project src/PackAndGo.Web
```
```bash
dotnet ef database update --project src/PackAndGo.Infrastructure --startup-project src/PackAndGo.Web
```

**Explanation**:

- **Creating a Migration**: This command generates a migration based on the changes made in the `AppDbContext`, including adding identity-related tables like `AspNetUsers` and renaming the `Users` table to `DomainUsers`.
- **Applying Migrations**: This command applies the migration, updating the database schema to include the necessary tables for both **ASP.NET Identity** and **Domain User** data.


### Defining Interfaces in the Application Layer

In this section, we will explore how the **Application Layer** provides the interfaces necessary for managing user authentication. These interfaces allow the higher layers (such as the **web layer**) to interact with authentication services and ensure that business logic remains decoupled from the **infrastructure layer**. We will also dive into the importance of the **Unit of Work** pattern and how it ensures consistency when working with multiple entities and repositories.

We define interfaces in the **Application Layer** that abstract the authentication functionality. These interfaces allow the **infrastructure layer** to provide specific implementations for authentication, while the **Application Layer** and other layers remain unaware of how authentication is handled under the hood.

Authentication should not be part of the Domain layer.

> `IAuthService.cs`

```csharp
namespace PackAndGo.Application.Interfaces;

public interface IAuthService
{
    Task RegisterUserAsync(string email);
    Task SignInAsync(string email);
    Task SignOutAsync();
}

```

**Explanation**:

- **IAuthService**: This interface defines the contract for managing user authentication operations. The actual implementation of these methods will be in the **Infrastructure Layer**, but the interface is in the **Application Layer** to ensure the higher layers (e.g., controllers) interact only with the abstraction, not the concrete implementation.
  
This interface defines the following methods:
- **`RegisterUserAsync(string email)`**: Registers a new user by creating both the **IdentityUser** (for authentication) and the **DomainUser** (for business logic).
- **`SignInAsync(string email)`**: Logs the user into the system by validating their credentials and establishing an authenticated session.
- **`SignOutAsync()`**: Logs the user out of the system and clears any session or authentication tokens.

By defining these methods in an interface, we allow flexibility in swapping or extending the authentication implementation without affecting the rest of the system.


### Implementing Transactional Integrity with the Unit of Work Pattern


When registering a new user, we need to ensure that both the **IdentityUser** (for authentication) and **DomainUser** (for domain logic) are created and persisted in a **single transaction**. If either part of the process fails, we should roll back the entire transaction to prevent data inconsistency.


The **Unit of Work (UoW)** pattern is designed to coordinate changes across multiple repositories in a single transaction. It ensures that changes made to the database are committed only when all operations succeed, and rolled back if an error occurs, maintaining data consistency.

> `IUnitOfWork.cs`

```csharp
namespace PackAndGo.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
    void Rollback(); 
}

```

**Explanation**:

- **IUnitOfWork**: This interface defines the contract for managing transactions across multiple database operations. It ensures that any changes made in the current scope are either fully committed or completely rolled back. This prevents inconsistent states where only some of the changes are saved.

This interface defines two methods:
- **`CommitAsync()`**: Commits all changes made during the transaction to the database. If successful, the changes are persisted.
- **`Rollback()`**: Rolls back any changes that were made during the transaction, effectively undoing all operations within the scope of the current transaction. In **EF Core**, rollback happens automatically if an exception is thrown, so this method is often just a safeguard.


**Why Do We Need Unit of Work?**

When multiple entities (such as **IdentityUser** and **DomainUser**) are involved in a user registration process, there is a risk of **partial completion**. If one entity is successfully persisted while another fails, this leaves the system in an inconsistent state, which could result in:
- Authentication data being saved, but the domain-specific user data missing.
- Incomplete or corrupted user records that can cause issues later in the system.

The **Unit of Work** pattern ensures that all changes across multiple repositories (e.g., **IdentityUser** and **DomainUser**) are part of a **single transaction**. This ensures **atomicity**, meaning that either **all operations succeed** or **none of them do**. If any step of the process fails (e.g., saving the domain user), the entire transaction is rolled back, preventing partial data persistence.

**Key Benefits of Using Unit of Work**:

1. **Transaction Management**: It wraps multiple repository operations (such as saving **IdentityUser** and **DomainUser**) in a single transaction. If one operation fails, the **entire transaction** is rolled back, ensuring data integrity.
2. **Consistency**: By ensuring that all related data changes are committed at the same time, the Unit of Work pattern guarantees that the system remains in a consistent state.
3. **Decoupling**: The **Application Layer** interacts only with the **IUnitOfWork** interface and does not need to know the specifics of how transactions are managed. This keeps the business logic focused on use cases, not infrastructure concerns.
4. **Error Handling**: Any exceptions raised during the process automatically trigger a rollback, simplifying error handling and ensuring that no partial data is saved.


### How Unit of Work is Applied in the Registration Process

When registering a user, we need to:

- **Create the IdentityUser**: This is the user record stored in the **ASP.NET Identity** tables for authentication.
- **Create the DomainUser**: This is the user record stored in the **DomainUsers** table for domain-specific logic.
- **Commit the transaction**: Once both the **IdentityUser** and **DomainUser** have been created, we commit the transaction to persist all changes. If anything fails during the process, we rollback to prevent partial data from being saved.

Here is how this looks in the registration process:

> `CookieAuthService.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PackAndGo.Application.Interfaces;
using PackAndGo.Domain.Entities;
using PackAndGo.Domain.Repositories;

namespace PackAndGo.Infrastructure.Security;

public class CookieAuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager;

    public CookieAuthService(IUserRepository userRepository,
                             IUnitOfWork unitOfWork,
                             IHttpContextAccessor httpContextAccessor,
                             UserManager<IdentityUser> userManager)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task RegisterUserAsync(string email)
    {
        // Step 1: Create the IdentityUser
        var identityUser = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        var identityResult = await _userManager.CreateAsync(identityUser);

        if (!identityResult.Succeeded)
        {
            // Handle failure (e.g., email already exists, invalid password, etc.)
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            throw new Exception($"Identity registration failed: {errors}");
        }

        // Step 2: Create and persist the domain user
        var domainUser = User.Create(email);
        await _userRepository.AddAsync(domainUser);

        // Step 3: Save changes with the UnitOfWork
        await _unitOfWork.CommitAsync(); // DbContext has automatic rollback on failure
    }

    public async Task SignInAsync(string email)
    {
        // Find user by email
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email.Value == email) ?? throw new Exception("User not found");

        // Find the IdentityUser by email
        var identityUser = await _userManager.FindByEmailAsync(email) ?? throw new Exception("Identity user not found");

        // Create claims from user email, identity and a principal
        // var claims = new[] { new Claim(ClaimTypes.Email, user.Email.Value) };
        var claims = new[] 
        {
            new Claim(ClaimTypes.Email, identityUser.Email!),
            new Claim(ClaimTypes.Name, identityUser.UserName!)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
        };

        // Sign in the user
        await _httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);
    }

    public async Task SignOutAsync()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}

```

**Explanation**:

- **RegisterUserAsync**: 
    - **Step 1**: We create an **IdentityUser** using **ASP.NET Identity** to handle authentication.
    - **Step 2**: We create a **DomainUser** for the business logic that lives in a separate database table.
    - **Step 3**: We call `CommitAsync()` on the **Unit of Work**, ensuring that both the **IdentityUser** and **DomainUser** are created in a single transaction. If any part of the process fails, the **Unit of Work** will rollback, preventing partial data from being saved.

This implementation demonstrates how the **Unit of Work** is critical in maintaining transactional integrity when dealing with multiple repositories or entities.

And here is the implementation of Unit Of Work itself:

> `UnitOfWork.cs`

```csharp
using PackAndGo.Application.Interfaces;

namespace PackAndGo.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync(); // Commit the transaction
    }

    public void Rollback()
    {
        // In EF Core, the transaction will automatically be rolled back if an exception is thrown.
        // You don't need an explicit rollback here unless you manually manage transactions.
    }
}
```


### Modifying the UserRepository for the Unit of Work Pattern

In this part of the chapter, we will focus on how the **UserRepository** needs to be adjusted to work properly with the **Unit of Work (UoW)** pattern. We'll also modify the repository to handle the renamed **DomainUsers** table and ensure that database operations are handled within the transaction managed by the **Unit of Work**. Finally, we'll walk through how to register the repository and authentication services in the **Dependency Injection (DI)** container.


The **UserRepository** currently contains calls to `SaveChangesAsync()` in methods like `AddAsync()`, `UpdateAsync()`, and `DeleteAsync()`. However, when using the **Unit of Work** pattern, we want the responsibility for committing changes to the database to be delegated to the **Unit of Work**, so that all operations can be grouped in a single transaction. If any step fails, the entire transaction should be rolled back.

We'll remove any direct calls to `SaveChangesAsync()` from the **UserRepository**, leaving the **Unit of Work** to manage saving changes across multiple repositories. This ensures that we maintain **transactional integrity** and all changes are committed together or not at all.

> `UserRepository.cs`

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
       var dataModel = await _context.DomainUsers.FindAsync(id);
       return dataModel == null ? null : dataModel.ToDomain();
   }

   public async Task<IEnumerable<User>> GetAllAsync()
   {
        return await _context.DomainUsers
            .Select(u => u.ToDomain())
            .ToListAsync();
   }

   public async Task AddAsync(User user)
   {
       var dataModel = UserDataModel.FromDomain(user);
       _context.DomainUsers.Add(dataModel);
       // await _context.SaveChangesAsync(); Handled by UnitOfWork
   }

   public async Task UpdateAsync(User user)
   {
        var dataModel = await _context.DomainUsers.FindAsync(user.Id);
        if (dataModel != null)
        {
            dataModel.Email = user.Email.Value;
            // await _context.SaveChangesAsync(); Handled by UnitOfWork
        }
   }

   public async Task DeleteAsync(Guid id)
   {
       var dataModel = await _context.DomainUsers.FindAsync(id);
       if (dataModel != null)
       {
           _context.DomainUsers.Remove(dataModel);
           // await _context.SaveChangesAsync(); Handled by UnitOfWork
       }
   }
}

```

**Explanation**:

- **Removal of `SaveChangesAsync()`**: We removed the direct calls to `SaveChangesAsync()` from `AddAsync()`, `UpdateAsync()`, and `DeleteAsync()`. The **Unit of Work** will now handle when to commit changes to the database.
- **Repository Focus**: The repository is now focused only on performing **CRUD** operations (adding, updating, and deleting data), while the **Unit of Work** manages transactions and saving changes.
- **Table Renaming**: We ensure that the repository now interacts with the `DomainUsers` table (the renamed version of the `Users` table) to prevent conflicts with **ASP.NET Identity**'s `AspNetUsers` table.


### Registering Services in the Dependency Injection Container

We need to ensure that the **repository**, **Unit of Work**, and **authentication services** are properly registered in the **Dependency Injection (DI)** container so they can be injected where needed (such as controllers or services).

We will modify the **ServiceRegistration** class to register the following services:

1. **AppDbContext**: The database context that handles both **Identity** and **Domain Users**.
2. **UserRepository**: The repository that performs CRUD operations for **Domain Users**.
3. **UnitOfWork**: The class responsible for managing database transactions and ensuring consistency across multiple operations.
4. **IAuthService**: The service that handles authentication and session management (sign-in, sign-out).

> `ServiceRegistration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PackAndGo.Domain.Repositories;
using PackAndGo.Infrastructure.Persistence;
using PackAndGo.Infrastructure.Repositories;
using PackAndGo.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using PackAndGo.Application.Interfaces;
using PackAndGo.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace PackAndGo.Infrastructure.Configuration;

public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind the "ConnectionStrings" section to DatabaseOptions
        services.Configure<DatabaseOptions>(configuration.GetSection("ConnectionStrings"));
        
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            // Retrieve the DefaultConnection string from the configured DatabaseOptions
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseSqlite(databaseOptions.DefaultConnection);
        });

        services.AddIdentityCore<IdentityUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Add authentication services
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	        .AddCookie();

        services.AddHttpContextAccessor();

        services.AddScoped<IAuthService, CookieAuthService>();
    }
}

```

**Explanation**:

1. **`AppDbContext` Registration**: We register the **AppDbContext** using a **SQLite** connection string pulled from the configuration. This context manages both the **ASP.NET Identity** tables (e.g., `AspNetUsers`, `AspNetRoles`) and the **DomainUsers** table.
   
2. **Identity Configuration**:
   - We use **AddIdentityCore** to register services for managing **IdentityUser** without requiring the entire ASP.NET Identity scaffolding (i.e., it focuses on core identity services without extra UI components).
   - The context is configured to store Identity data using **Entity Framework** by calling `AddEntityFrameworkStores<AppDbContext>()`.

3. **Repository and Unit of Work**:
   - We register the **UserRepository** and **UnitOfWork** as **scoped services**, meaning a new instance will be created for each request.
   - The **UserRepository** is responsible for domain-specific user operations, while the **UnitOfWork** ensures that all database changes are handled transactionally.

4. **Authentication Configuration**:
   - We register **cookie-based authentication** using `AddCookie()` to manage authentication sessions.
   - The **IAuthService** is registered to the **CookieAuthService**, which will handle the sign-in, sign-out, and user registration processes.


### Implementing the GUI in the Web Project

In this section, we will focus on implementing the **GUI** for user registration, login, and logout functionality in the **web project**. We'll build the user interface using **ASP.NET Core MVC** and integrate it with the authentication services we defined earlier. We will create a registration form, handle user input validation, and manage redirection upon successful registration or login.


How can we expose user registration and login functionality to the end user while ensuring data validation and secure handling of credentials?

We'll implement a simple and user-friendly **MVC-based** registration and login interface. This will include:

- A registration form for creating new users.
- A login form for authenticating users.
- Proper validation of user inputs, such as email format, password length, and matching confirmation passwords.
- Display of error messages for invalid input or registration failures.

The following sections outline the creation of the **view models**, **controllers**, and **Razor views** required for user registration and authentication.


#### View Model for User Registration

The **`RegisterUserViewModel`** defines the fields required for user registration and includes validation attributes to ensure correct data input. This model will be used to bind data from the registration form and perform server-side validation.

> `RegisterUserViewModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace PackAndGo.Web.Models;

public class RegisterUserViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Confirm Password is required.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    public string? LastName { get; set; }
}

```

**Explanation**:

- **Validation**: The **`RegisterUserViewModel`** includes validation attributes such as `[Required]`, `[EmailAddress]`, and `[Compare]` to ensure the user enters a valid email, matching passwords, and both first and last names. This ensures that only valid data is submitted to the controller.
- **Form Binding**: The properties in this view model are used to capture the data entered in the registration form and passed to the controller for processing.

#### AccountController: Handling Registration and Authentication

The **`AccountController`** handles user registration, login, and logout actions. It interacts with the **`IAuthService`** to register new users and authenticate existing users. The controller also includes error handling and redirects users after successful registration or login.

> `AccountController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackAndGo.Application.Interfaces;
using PackAndGo.Web.Models;

namespace PackAndGo.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult RegisterUser()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterUserAsync(RegisterUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model); // Return the view with validation errors if the model is invalid
        }

        try
        {
            await _authService.RegisterUserAsync(model.Email!); // Register the user
            return RedirectToAction("Index", "Home"); // Redirect to a different page upon successful registration
        }
        catch (Exception ex)
        {
            // Handle errors (e.g., user already exists, registration failed, etc.)
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model); // Show the error in the view
        }
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken] // This ensures that the form is submitted with a valid anti-forgery token to prevent CSRF attacks.
    public async Task<IActionResult> LoginAsync(LoginViewModel model)
    {
        // Check if the model is valid
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        if (model.Email == null || model.Password == null)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View();
        }

        // Attempt to sign in the user
        try
        {
            await _authService.SignInAsync(model.Email);
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }
    }

    public async Task<IActionResult> LogoutAsync()
    {
        await _authService.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize] // This attribute ensures that only authenticated users can access the action.
    public IActionResult SecretInfo()
    {
        return View();
    }

}

```

**Explanation**:

- **`RegisterUser()`**: Displays the user registration form.
- **`RegisterUserAsync()`**: Handles form submissions, performs validation, and interacts with **`IAuthService.RegisterUserAsync()`** to register the user. On success, the user is redirected to the home page.
- **`Login()`**: Displays the login form.
- **`LoginAsync()`**: Validates user credentials and signs them in via **`IAuthService.SignInAsync()`**. If login fails, an error message is shown.
- **`LogoutAsync()`**: Signs out the user using **`IAuthService.SignOutAsync()`** and redirects them to the home page.
- **`SecretInfo()`**: An example of a restricted page that only authenticated users can access, enforced via the `[Authorize]` attribute.

#### Razor View for User Registration

The **Razor View** for user registration provides a form for users to enter their details (email, password, first name, and last name). It integrates client-side validation via ASP.NET MVC helpers.

> `RegisterUser.cshtml`

```html
@model PackAndGo.Web.Models.RegisterUserViewModel

@{
    ViewData["Title"] = "Register User";
}

<h2>Register</h2>

<form asp-action="RegisterUser" method="post">
    <div class="form-group">
        <label asp-for="Email"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="Password"></label>
        <input asp-for="Password" class="form-control" />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="ConfirmPassword"></label>
        <input asp-for="ConfirmPassword" class="form-control" />
        <span asp-validation-for="ConfirmPassword" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="FirstName"></label>
        <input asp-for="FirstName" class="form-control" />
        <span asp-validation-for="FirstName" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="LastName"></label>
        <input asp-for="LastName" class="form-control" />
        <span asp-validation-for="LastName" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">Register</button>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

```

**Explanation**:

- **Form Input**: Each field (email, password, confirm password, first name, last name) is bound to the **`RegisterUserViewModel`** and uses ASP.NET MVC helpers like **`asp-for`** and **`asp-validation-for`** to display the form and validation messages.
- **Client-Side Validation**: The **`_ValidationScriptsPartial`** section ensures that client-side validation is included using unobtrusive JavaScript validation.


#### Adding a Register User Button to the Navigation

Finally, we add a navigation button to the layout to provide easy access to the registration page.

> `_Layout.cshtml`

```html
<li class="nav-item">
    <a class="nav-link text-dark" asp-area="" asp-controller="Account" asp-action="RegisterUser">Register User</a>
</li>
```

**Explanation**:

- **Register User Link**: This link is added to the site's navigation menu and directs users to the **RegisterUser** action of the **AccountController**, which renders the registration form.


### Summary and Learning Outcomes

In this chapter, we implemented **ASP.NET Core Identity** for user authentication while keeping the **domain logic** separate, adhering to **clean architecture** and **Domain-Driven Design (DDD)** principles. Our goal was to maintain a clear distinction between authentication, managed in the **Infrastructure Layer**, and business-specific user logic, residing in the **Domain Layer**.

We began by setting up the **`AppDbContext`** to handle both **IdentityUser** for authentication and **DomainUser** for domain logic, ensuring the two are stored in separate tables to avoid conflicts. By renaming the `Users` table to `DomainUsers`, we maintained a clean separation of concerns, ensuring the domain model is independent of authentication mechanisms.

The **Unit of Work (UoW)** pattern was introduced to ensure transactional integrity when saving both **IdentityUser** and **DomainUser** in a single transaction. This prevents partial data saves, ensuring that both entities are either fully committed or rolled back in the event of an error. We also modified the **UserRepository** to delegate transaction management to the **Unit of Work**, ensuring consistency across multiple operations.

