## Implementing Cookie-Based Authentication in a Clean Architecture ASP.NET Application

In this chapter, we will implement basic cookie-based authentication for an ASP.NET Core web application using **Clean Architecture** and **Domain-Driven Design (DDD)** principles. The goal is to authenticate a user based solely on their email address. Password handling will be added in future iterations, focusing on simplicity and adherence to architectural guidelines. This approach adheres to the separation of concerns between layers, with authentication logic in the **Infrastructure Layer** and business rules defined in the **Application Layer**.

### Understanding the Authentication Flow

Cookie authentication is a standard approach where a cookie is issued to the client upon successful authentication and sent with every request to authenticate the user. In this implementation, we will use only the email as the user’s identifier and store that in the cookie.

To adhere to **Clean Architecture**:
- The **Application Layer** will manage the authentication logic, defined in a service interface.
- The **Domain Layer** remains independent of the authentication logic, responsible only for core entities like `User`.
- The **Infrastructure Layer** will provide the concrete implementation for cookie authentication and integration with the ASP.NET Core pipeline.
- The Web Layer is kept thin
### Directory Structure

We organize the solution into distinct layers, each with a specific responsibility. This is how the **Clean Architecture** directory structure looks:

```
src/
├── PackAndGo.Application/
│   ├── Interfaces/
│   │   └── IAuthService.cs
├── PackAndGo.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Repositories/
├── PackAndGo.Infrastructure/
│   ├── Configuration/
│   │   └── ServiceRegistration.cs
│   ├── Security/
│   │   └── CookieAuthService.cs
├── PackAndGo.Web/
│   ├── Program.cs
│   ├── Models/
│   │   └── LoginViewModel.cs
│   ├── Controllers/
│   │   └── AccountController.cs
│   ├── Views/
│   │   └── Account/
│           └── Login.cshtml
│           └── SecretInfo.cshtml
```

### Step 1: Defining the Authentication Interface in the Application Layer

In the **Application Layer**, we define an interface for the authentication service. This interface abstracts the authentication process, decoupling it from the actual implementation, which will reside in the **Infrastructure Layer**.

> `src/PackAndGo.Application/Interfaces/IAuthService.cs`

```csharp
namespace PackAndGo.Application.Interfaces;

public interface IAuthService
{
    Task SignInAsync(string email);
    Task SignOutAsync();
}
```

### Step 2: Implementing Cookie Authentication in the Infrastructure Layer

The actual authentication logic, which includes setting the authentication cookie, will be implemented in the **Infrastructure Layer**. Here, we use the `IHttpContextAccessor` to interact with the current `HttpContext` and manage the cookie.

Ensure that all the necessary project references and NuGet packages are installed:

#### Add References to the Infrastructure Project

```bash
dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj reference src/PackAndGo.Application/PackAndGo.Application.csproj
```

#### Install Required NuGet Packages

```bash
# Install cookie authentication and related packages
dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.Cookies
dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj package Microsoft.AspNetCore.Http.Abstractions
```

> `src/PackAndGo.Infrastructure/Security/CookieAuthService.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using PackAndGo.Application.Interfaces;
using PackAndGo.Domain.Repositories;

namespace PackAndGo.Infrastructure.Security;

public class CookieAuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieAuthService(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SignInAsync(string email)
    {
        // Find user by email
        var users = await _userRepository.GetAllAsync();
        var user = users.FirstOrDefault(u => u.Email.Value == email) ?? throw new Exception("User not found");

        // Create claims from user email, identity and a principal
        var claims = new[] { new Claim(ClaimTypes.Email, user.Email.Value) };
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

### Step 3: Registering Authentication Services in the Infrastructure Layer

We need to register both `CookieAuthService` and the necessary authentication middleware in the **Infrastructure Layer**. This is done in the `ServiceRegistration` class, where we configure cookie authentication and set up the HTTP context accessor.

> `src/PackAndGo.Infrastructure/Configuration/ServiceRegistration.cs`

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

        services.AddScoped<IUserRepository, UserRepository>();

        // Add authentication services
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	        .AddCookie();

        services.AddHttpContextAccessor();

        services.AddScoped<IAuthService, CookieAuthService>();
    }
}
```

### Step 4: Configuring Middleware in the Web Layer

For the authentication process to work, the **Web Layer** must include the appropriate authentication and authorization middleware. This ensures that the authentication cookie is processed correctly on each request.

> `src/PackAndGo.Web/Program.cs`

```csharp
using PackAndGo.Application.Configuration;
using PackAndGo.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register services from Application and Infrastructure layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### Step 5: Adding the ViewModel for Login

To validate user input, we use a `LoginViewModel` to handle form data.

> `src/PackAndGo.Web/Models/LoginViewModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace PackAndGo.Web.Models;

public class LoginViewModel
{
    [Required]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
```

### Step 6: Implementing the Login and Logout Functionality in the Web Layer

Next, we implement a controller to handle user login and logout actions. The controller interacts with the `IAuthService` to manage authentication.

> `src/PackAndGo.Web/Controllers/AccountController.cs`

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

### Step 7: Adding the Login View

The `Login.cshtml` view provides a form for the user to input their email and login to the application.

> `src/PackAndGo.Web/Views/Account/Login.cshtml`

```html
@model PackAndGo.Web.Models.LoginViewModel

<h2>Login</h2>

<form asp-action="Login" asp-controller="Account" method="post" asp-antiforgery="true">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <div>
        <label asp-for="Email">Email:</label>
        <input type="text" id="Email" asp-for="Email" required />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>

    <div>
        <label asp-for="Password">Password:</label>
        <input type="password" id="Password" asp-for="Password" required />
        <span asp-validation-for="Password" class="text-danger"></span>
    </div>

    <div>
        <button type="submit">Login</button>
    </div>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

> `src/PackAndGo.Web/Views/Account/SecretInfo.cshtml`

```html
<h2>Authentication Info</h2>

<table class="table">
    <thead>
        <tr>
            <th>Claim Type</th>
            <th>Claim Value</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var claim in User.Claims)
        {
            <tr>
                <td>@claim.Type</td>
                <td>@claim.Value</td>
            </tr>
        }
    </tbody>
</table>
```

> `src/PackAndGo.Web/Views/Shared/_Layout.cshtml`

```html
<li class="nav-item">
    @if (Context?.User?.Identity?.IsAuthenticated == true)
    {
        <a class="nav-link text-dark" asp-area="" asp-controller="Account" asp-action="Logout">Logout</a>
    }
    else
    {
        <a class="nav-link text-dark" asp-area="" asp-controller="Account" asp-action="Login">Login</a>
    }
</li>
<li class="nav-item">
    <a class="nav-link text-dark" asp-area="" asp-controller="Account" asp-action="SecretInfo">Secret Info</a>
</li>

```


### Summary

In this chapter, we successfully implemented cookie-based authentication in an ASP.NET Core application following **Clean Architecture** principles. The **Application Layer** defines the `IAuthService` interface, while the **Infrastructure Layer** provides the implementation via `CookieAuthService`. The **Web Layer** includes the authentication middleware and controllers that interact with users.

### Learning Outcomes

By the end of this chapter, you should:

1. Understand how to implement cookie-based authentication in a Clean Architecture ASP.NET application.
2. Recognize the importance of separating interface definitions in the

 **Application Layer** and implementation in the **Infrastructure Layer**.
3. Know how to configure ASP.NET Core middleware for authentication.
4. Understand how to create login functionality and manage authentication cookies.

This basic authentication system can be expanded later to include passwords, more secure authentication mechanisms, and role-based authorization.