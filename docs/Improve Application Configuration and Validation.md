## Improve Application Configuration and Validation

In this chapter, we'll enhance our existing ASP.NET Core web application by applying several important best practices. Specifically, we will:

1. **Use Extension Methods** to decouple the registration of services from the `Web` project, reducing dependencies between layers.
2. **Implement the Options Pattern** for managing the database connection string, providing a more flexible and maintainable approach to configuration.
3. **Add Client-Side Validation** for email, improving the user experience by catching validation errors early.

By the end of this chapter, your application will be better organized, more maintainable, and offer an improved user experience.

### Registering Services with Extension Methods

In the current setup, services from the `Application` and `Infrastructure` layers are registered directly within the `Web` project. This approach creates tight coupling between these layers, making the `Web` project dependent on the implementation details of other layers. We can decouple these layers by using extension methods to register services, which also promotes better separation of concerns.

#### Creating the Extension Methods

Let's start by creating extension methods for registering services in both the `Application` and `Infrastructure` layers. We'll place these extension methods in a `Configuration` subfolder to better organize our code.

1. **Application Layer - `ServiceRegistration.cs`:**

	First, ensure you have the necessary NuGet packages installed for the `Application` layer. Run the following command from the solution root folder:
	
	```bash
	dotnet add src/PackAndGo.Application/PackAndGo.Application.csproj package Microsoft.Extensions.DependencyInjection
	```
	
	Next, create a `Configuration` folder within the `Application` project and add the `ServiceRegistration.cs` file:
	
	```csharp
	using Microsoft.Extensions.DependencyInjection;
	using PackAndGo.Application.Interfaces;
	using PackAndGo.Application.Services;
	
	namespace PackAndGo.Application.Configuration;
	
	public static class ServiceRegistration
	{
	    public static void AddApplicationServices(this IServiceCollection services)
	    {
	        services.AddScoped<IUserService, UserService>();
	    }
	}
	```


2. **Infrastructure Layer - Implementing the Options Pattern for Database Configuration**

	The options pattern is a recommended approach for managing application configuration, such as connection strings. It allows for strongly typed access to settings and facilitates testing and management.
	
	**Creating the `DatabaseOptions` Class**
	
	First, create a `DatabaseOptions` class in the `Infrastructure` project to represent the database configuration. Place this class in an `Options` folder:
	
	```csharp
	namespace PackAndGo.Infrastructure.Options;
	
	public class DatabaseOptions
	{
	    public string DefaultConnection { get; set; } = string.Empty;
	}
	```

3. **Infrastructure Layer - `ServiceRegistration.cs`:**

	Next, modify the infrastructure service registration to use the options pattern. The `AddInfrastructureServices` method now configures and retrieves the connection string using the options pattern.
	
	For the `Infrastructure` layer, ensure you have the necessary NuGet packages installed by running the following commands from the solution root folder:
	
	```bash
	dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection
	dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj package Microsoft.EntityFrameworkCore
	dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj package Microsoft.Extensions.Options.ConfigurationExtensions
	```
	
	Now, create a `Configuration` folder within the `Infrastructure` project and add the `ServiceRegistration.cs` file:
	
	```csharp
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Options;
	using PackAndGo.Domain.Repositories;
	using PackAndGo.Infrastructure.Persistence;
	using PackAndGo.Infrastructure.Repositories;
	using PackAndGo.Infrastructure.Options;
	
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
	    }
	}
	```

4. **Refactoring the Program.cs**

	Now, let's modify `Program.cs` to use these extension methods, removing the direct dependencies on `Application` and `Infrastructure` services:
	
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
	    app.UseHsts();
	}
	
	app.UseHttpsRedirection();
	app.UseStaticFiles();
	
	app.UseRouting();
	
	app.UseAuthorization();
	
	app.MapControllerRoute(
	    name: "default",
	    pattern: "{controller=Home}/{action=Index}/{id?}");
	
	app.Run();
	```
	

### Adding Client-Side Validation for Email

Client-side validation provides immediate feedback to users, enhancing the user experience by preventing form submission if invalid data is detected.

1. **Updating the `UserViewModel`**

	Start by updating the `UserViewModel` in the `Web` project to include validation attributes:
	
	```csharp
	using System.ComponentModel.DataAnnotations;
	
	namespace PackAndGo.Web.Models;
	
	public class UserViewModel
	{
	    public Guid Id { get; set; }
	
	    [Required(ErrorMessage = "Email is required.")]
	    [EmailAddress(ErrorMessage = "Invalid email address.")]
	    public string Email { get; set; } = string.Empty;
	}
	```

2. **Enabling Client-Side Validation**

Ensure that client-side validation is enabled in your Razor views by including the necessary validation scripts in `Create.cshtml` and `Edit.cshtml`:

> `Create.cshtml`

```html
@model PackAndGo.Web.Models.UserViewModel

<h1>Create User</h1>

@if (!ViewData.ModelState.IsValid)
{
  <div>
    @foreach (var error in ViewData.ModelState.Values.SelectMany(v => v.Errors))
    {
      <p>@error.ErrorMessage</p>
    }
  </div>
}

<form asp-action="Create" method="post">
  <div>
    <label asp-for="Email"></label>
    <input asp-for="Email" class="form-control" />
    <span asp-validation-for="Email" class="text-danger"></span>
  </div>
  <div>
    <input type="submit" value="Create" class="btn btn-primary" />
  </div>
</form>

<p>
  <a href="@Url.Action("Index")">Back to List</a>
</p>

@section Scripts {
  @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

This includes the `_ValidationScriptsPartial`, which loads the necessary JavaScript files for client-side validation.


> `Edit.cshtml`

```html
@model PackAndGo.Web.Models.UserViewModel

<h1>Edit User</h1>

@if (!ViewData.ModelState.IsValid)
{
  <div>
    @foreach (var error in ViewData.ModelState.Values.SelectMany(v => v.Errors))
    {
      <p>@error.ErrorMessage</p>
    }
  </div>
}

<form asp-action="Edit" method="post">
  <input type="hidden" asp-for="Id" />
  <div>
    <label asp-for="Email"></label>
    <input asp-for="Email" class="form-control" />
    <span asp-validation-for="Email" class="text-danger"></span>
  </div>
  <div>
    <input type="submit" value="Save" class="btn btn-primary" />
  </div>
</form>

<p>
  <a href="@Url.Action("Index")">Back to List</a>
</p>

@section Scripts {
  @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

**Explanation of Changes**

1. **Client-Side Validation**: The `asp-validation-for` tag helper and the `Scripts` section now include client-side validation by rendering the necessary scripts using `_ValidationScriptsPartial`. This will provide immediate feedback to users as they fill out the form.

2. **Retained Server-Side Validation**: The existing logic that checks `ModelState.IsValid` and displays errors in case of server-side validation failures remains intact. This ensures that any validation errors caught on the server will be displayed to the user, even if client-side validation is bypassed or not supported.

3. **Form Elements**:
   - **`asp-for`** and **`asp-validation-for`**: These tag helpers bind the input fields and validation spans to the model properties, ensuring both client-side and server-side validations are appropriately linked.

4. **Bootstrap Classes** (optional): We’ve added some basic Bootstrap classes like `form-control` and `btn btn-primary` for better styling. If you’re not using Bootstrap, you can remove or replace these classes.





### Summary

In this chapter, we enhanced our application by:

1. **Decoupling Service Registrations**: Using extension methods in the `Configuration` subfolder to register services in the `Application` and `Infrastructure` layers, reducing direct dependencies between `Web` and `Domain`.
2. **Implementing the Options Pattern**: Managing the database connection string using the options pattern, which improves maintainability and testability.
3. **Adding Client-Side Validation**: Improving the user experience with immediate feedback by validating the email address on the client side.

By following these practices, you’ve improved your application's architecture, configuration management, and user interface, setting the stage for more advanced features and enhancements.

This chapter showcases how to progressively refine an application by adopting best practices, which makes it more maintainable, scalable, and user-friendly.