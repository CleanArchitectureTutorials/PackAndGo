## Start Implementing The App

Let's start implementing our app step by step following the Clean Architecture principles. We'll start by modeling a `User` entity and corresponding classes in each layer, then implement the database context and CRUD operations. Finally, we'll build the necessary services and controllers. At the end of this chapter you will have a very minimalistic implementation of a working app. In this first iteration we will use SQLite as database.

### Define the Domain Layer

1. **Create the `User` Entity**

   In the `PackAndGo.Domain` project, create a new folder called `Entities`. Inside this folder, create a `User.cs` file and define the `User` entity:

   ```csharp
   namespace PackAndGo.Domain.Entities;

   public class User
   {
       public Guid Id { get; private set; }
       public string Email { get; private set; }

       public User(Guid id, string email)
       {
           Id = id;
           Email = email;
       }
   }
   ```

   This entity represents the core user information in the domain.

2. **Define the `IUserRepository` Interface**

   In order to persist the User into a database we will use the _Repository Pattern_. This will help us to separate the fact that we want to persist the User from how it is implemented. The implementation of this interface is done in the _Infrastructure_ layer.
   
   Create another folder in the `Domain` project called `Repositories` and add an `IUserRepository.cs` file:

   ```csharp
   using PackAndGo.Domain.Entities;

	namespace PackAndGo.Domain.Repositories;

   public interface IUserRepository
   {
       Task<User?> GetByIdAsync(Guid id);
       Task<IEnumerable<User>> GetAllAsync();
       Task AddAsync(User user);
       Task UpdateAsync(User user);
       Task DeleteAsync(Guid id);
   }
   ```

   This interface defines the CRUD operations for the `User` entity.

### Define the Infrastructure Layer

1. **Create the `UserDataModel`**

   In the `PackAndGo.Infrastructure` project, create a folder named `DataModels`, and inside it, add a `UserDataModel.cs` file:

   ```csharp
   namespace PackAndGo.Infrastructure.DataModels;

   public class UserDataModel
   {
       public Guid Id { get; set; }
       public string Email { get; set; } = string.Empty;
   }
   ```

   This data model represents the `User` entity for database interactions.

2. **Install the Entity Framework Core SQLite NuGet packages** by running the following commands from the solution root folder:

   ```bash
   dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
   ``` 

	These commands will install the necessary EF Core SQLite packages in the `PackAndGo.Infrastructure` project.

3. **Implement the `AppDbContext`**

   Next, create a folder named `Persistence` in the `Infrastructure` project and add an `AppDbContext.cs` file:

   ```csharp
   using Microsoft.EntityFrameworkCore;
   using PackAndGo.Infrastructure.DataModels;

   namespace PackAndGo.Infrastructure.Persistence;

   public class AppDbContext : DbContext
   {
       public DbSet<UserDataModel> Users { get; set; }

       public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
       {
           modelBuilder.Entity<UserDataModel>().HasData(
               new UserDataModel { Id = Guid.NewGuid(), Email = "john.doe@test.com" },
               new UserDataModel { Id = Guid.NewGuid(), Email = "jane.doe@test.com" },
               new UserDataModel { Id = Guid.NewGuid(), Email = "jacob.doe@test.com" }
           );
       }
   }
   ```

   The `AppDbContext` will manage the database operations and include seeding for initial data.

4. **Implement the `UserRepository`**

   In the `Infrastructure` project, create a `Repositories` folder and add a `UserRepository.cs` file:

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
           return dataModel == null ? null : new User(dataModel.Id, dataModel.Email);
       }

       public async Task<IEnumerable<User>> GetAllAsync()
       {
           return await _context.Users
               .Select(u => new User(u.Id, u.Email))
               .ToListAsync();
       }

       public async Task AddAsync(User user)
       {
           var dataModel = new UserDataModel
           {
               Id = user.Id,
               Email = user.Email
           };
           _context.Users.Add(dataModel);
           await _context.SaveChangesAsync();
       }

       public async Task UpdateAsync(User user)
       {
           var dataModel = await _context.Users.FindAsync(user.Id);
           if (dataModel != null)
           {
               dataModel.Email = user.Email;
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

   This repository implements the `IUserRepository` interface using Entity Framework Core to interact with the database.

### Define the Application Layer

1. **Create the `UserDTO`**

   In the `PackAndGo.Application` project, create a `DTOs` folder and add a `UserDTO.cs` file:

   ```csharp
   namespace PackAndGo.Application.DTOs;

   public class UserDTO
   {
       public Guid Id { get; set; }
       public string Email { get; set; } = string.Empty;
   }
   ```

   This DTO represents the data transfer object for the `User` entity.

2. **Create the `IUserService` Interface**

  In the `PackAndGo.Application` project, create a folder called `Interfaces` and add an `IUserService.cs` file:

  ```csharp
  using PackAndGo.Application.DTOs;

  namespace PackAndGo.Application.Interfaces;

  public interface IUserService
  {
      Task<UserDTO?> GetUserByIdAsync(Guid id);
      Task<IEnumerable<UserDTO>> GetAllUsersAsync();
      Task AddUserAsync(UserDTO userDto);
      Task UpdateUserAsync(UserDTO userDto);
      Task DeleteUserAsync(Guid id);
  }
  ```
  
  This interface defines the contract for the user service, ensuring consistency and testability.

3. **Implement the `UserService`**

   Next, in the `Application` project, create a `Services` folder and add a `UserService.cs` file:

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
           return user == null ? null : new UserDTO { Id = user.Id, Email = user.Email };
       }

       public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
       {
           return (await _userRepository.GetAllAsync())
               .Select(user => new UserDTO { Id = user.Id, Email = user.Email });
       }

       public async Task AddUserAsync(UserDTO userDto)
       {
           var user = new User(userDto.Id, userDto.Email);
           await _userRepository.AddAsync(user);
       }

       public async Task UpdateUserAsync(UserDTO userDto)
       {
           var user = new User(userDto.Id, userDto.Email);
           await _userRepository.UpdateAsync(user);
       }

       public async Task DeleteUserAsync(Guid id)
       {
           await _userRepository.DeleteAsync(id);
       }
   }
   ```

   The `UserService` provides application logic by interacting with the domain layer via the repository.

### Define the Web Layer

1. **Create the `UserViewModel`**

   In the `PackAndGo.Web` project, go to the `Models` folder and add a `UserViewModel.cs` file:

   ```csharp
   namespace PackAndGo.Web.Models;

   public class UserViewModel
   {
       public Guid Id { get; set; }
       public string Email { get; set; } = string.Empty;
   }
   ```

   This ViewModel will be used in the views to present user data.

2. **Implement the `UserController`**

   In the `Controllers` folder of the `Web` project, add a `UserController.cs` file:

   ```csharp
   using Microsoft.AspNetCore.Mvc;
   using PackAndGo.Application.DTOs;
   using PackAndGo.Application.Interfaces;
   using PackAndGo.Web.Models;

   namespace PackAndGo.Web.Controllers;

   public class UserController : Controller
   {
       private readonly IUserService _userService;

       public UserController(IUserService userService)
       {
           _userService = userService;
       }

       public async Task<IActionResult> Index()
       {
           var users = await _userService.GetAllUsersAsync();
           var viewModel = users.Select(u => new UserViewModel { Id = u.Id, Email = u.Email });
           return View(viewModel);
       }

       public IActionResult Create()
       {
           return View();
       }

       [HttpPost]
       public async Task<IActionResult> Create(UserViewModel model)
       {
           if (ModelState.IsValid)
           {
               var userDto = new UserDTO { Id = Guid.NewGuid(), Email = model.Email };
               await _userService.AddUserAsync(userDto);
               return RedirectToAction(nameof(Index));
           }
           return View(model);
       }

       public async Task<IActionResult> Edit(Guid id)
       {
           var user = await _userService.GetUserByIdAsync(id);
           if (user == null) return NotFound();

           var viewModel = new UserViewModel { Id = user.Id, Email = user.Email };
           return View(viewModel);
       }

       [HttpPost]
       public async Task<IActionResult> Edit(UserViewModel model)
       {
           if (ModelState.IsValid)
           {
               var userDto = new UserDTO { Id = model.Id, Email = model.Email };
               await _userService.UpdateUserAsync(userDto);
               return RedirectToAction(nameof(Index));
           }
           return View(model);
       }

       public async Task<IActionResult> Delete(Guid id)
       {
           var user = await _userService.GetUserByIdAsync(id);
           if (user == null) return NotFound();

           return View(new UserViewModel { Id = user.Id, Email = user.Email });
       }

       [HttpPost, ActionName("Delete")]
       public async Task<IActionResult> DeleteConfirmed(Guid id)
       {
           await _userService.DeleteUserAsync(id);
           return RedirectToAction(nameof(Index));
       }
   }
   ```

   The `UserController` handles the CRUD operations for users, interacting with the `UserService` and utilizing the `UserViewModel`.

3. **Create the Views**

   These views use the `UserViewModel` and provide basic forms for user interaction.

	In the `Views/User` folder of the `PackAndGo.Web` project, create the following Razor views:
	
	- **Index.cshtml**: List all users.
	
	  ```html
	  @model IEnumerable<PackAndGo.Web.Models.UserViewModel>
	
	  <h1>Users</h1>
	
	  <p>
	      <a href="@Url.Action("Create")">Create New User</a>
	  </p>
	
	  <table>
	      <thead>
	          <tr>
	              <th>Email</th>
	              <th></th>
	          </tr>
	      </thead>
	      <tbody>
	          @foreach (var user in Model)
	          {
	              <tr>
	                  <td>@user.Email</td>
	                  <td>
	                      <a href="@Url.Action("Edit", new { id = user.Id })">Edit</a> |
	                      <a href="@Url.Action("Delete", new { id = user.Id })">Delete</a>
	                  </td>
	              </tr>
	          }
	      </tbody>
	  </table>
	  ```
	
	- **Create.cshtml**: Form to create a new user.
	
	  ```html
	  @model PackAndGo.Web.Models.UserViewModel
	
	  <h1>Create User</h1>
	
	  <form asp-action="Create" method="post">
	      <div>
	          <label>Email</label>
	          <input asp-for="Email" />
	      </div>
	      <div>
	          <input type="submit" value="Create" />
	      </div>
	  </form>
	
	  <p>
	      <a href="@Url.Action("Index")">Back to List</a>
	  </p>
	  ```
	
	- **Edit.cshtml**: Form to edit an existing user.
	
	  ```html
	  @model PackAndGo.Web.Models.UserViewModel
	
	  <h1>Edit User</h1>
	
	  <form asp-action="Edit" method="post">
	      <input type="hidden" asp-for="Id" />
	      <div>
	          <label>Email</label>
	          <input asp-for="Email" />
	      </div>
	      <div>
	          <input type="submit" value="Save" />
	      </div>
	  </form>
	
	  <p>
	      <a href="@Url.Action("Index")">Back to List</a>
	  </p>
	  ```
	
	- **Delete.cshtml**: Confirmation page to delete a user.
	
	  ```html
	  @model PackAndGo.Web.Models.UserViewModel
	
	  <h1>Delete User</h1>
	
	  <form asp-action="Delete" method="post">
	      <input type="hidden" asp-for="Id" />
	      <p>Are you sure you want to delete this user: @Model.Email?</p>
	      <div>
	          <input type="submit" value="Delete" />
	      </div>
	  </form>
	
	  <p>
	      <a href="@Url.Action("Index")">Back to List</a>
	  </p>
	  ```

	- **Add the "Users" Menu Item in the Shared `_Layout.cshtml`**
	
	  In the `Views/Shared` folder of the `PackAndGo.Web` project, open the `_Layout.cshtml` file and add a "Users" menu item to the navigation bar:
	
	  ```html
	  <ul class="navbar-nav">
	      <li class="nav-item">
	          <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">Home</a>
	      </li>
	      <li class="nav-item">
	          <a class="nav-link text-dark" asp-area="" asp-controller="User" asp-action="Index">Users</a>
	      </li>
	  </ul>
	  ```
	
	  This will add a "Users" link to the navigation menu, allowing users to easily navigate to the `User` management page.


	These views are simple and functional, providing the basic UI elements needed to perform CRUD operations on the `User` entity. They focus on functionality without any additional styling.
	

4. **Register Services and Repositories in the Dependency Injection (DI) Container**

	In the `PackAndGo.Web` project, open the `Program.cs` file and register the necessary services and repositories in the DI container. We need the `using` statements and the code between the dashes (`// ---`)
	
	```csharp
	using Microsoft.EntityFrameworkCore;
	using PackAndGo.Application.Interfaces;
	using PackAndGo.Application.Services;
	using PackAndGo.Domain.Repositories;
	using PackAndGo.Infrastructure.Persistence;
	using PackAndGo.Infrastructure.Repositories;

	var builder = WebApplication.CreateBuilder(args);
	
	// Add services to the container.
	builder.Services.AddControllersWithViews();
	
	// --- Here is the code that registers our services
	
	// Register the DbContext
	builder.Services.AddDbContext<AppDbContext>(options =>
	  options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
	
	// Register repositories
	builder.Services.AddScoped<IUserRepository, UserRepository>();
	
	// Register services
	builder.Services.AddScoped<IUserService, UserService>();
	
	// --- End of code
	
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
	
	app.UseAuthorization();
	
	app.MapControllerRoute(
	    name: "default",
	    pattern: "{controller=Home}/{action=Index}/{id?}");
	
	app.Run();
	```
	
	This ensures that the `AppDbContext`, `IUserRepository`, and `IUserService` are properly registered with the DI container and can be injected where needed in the application.
	  
  You can add the following bullet point when configuring the SQLite database connection string:

5. **Set the SQLite Connection String in `appsettings.json`**

  In the `PackAndGo.Web` project, open the `appsettings.json` file and add the SQLite connection string:

	```json
	{
	  "ConnectionStrings": {
	    "DefaultConnection": "Data Source=../../PackAndGo.db"
	  },
	  "Logging": {
	    "LogLevel": {
	      "Default": "Information",
	      "Microsoft.AspNetCore": "Warning"
	    }
	  },
	  "AllowedHosts": "*"
	}
	```

  This configuration specifies that the SQLite database file will be named `PackAndGo.db` and will be located in the solution root of the application.
  
### Create the Database

1. **Run the Migration and Create the SQLite Database**

  From the solution root folder, use the following commands to add a database migration and update the database. This will create the SQLite database based on the `AppDbContext`:

  ```bash
  dotnet add src/PackAndGo.Web/PackAndGo.Web.csproj package Microsoft.EntityFrameworkCore.Design
  dotnet ef migrations add InitialCreate --project src/PackAndGo.Infrastructure --startup-project src/PackAndGo.Web
  dotnet ef database update --project src/PackAndGo.Infrastructure --startup-project src/PackAndGo.Web
  ```

  These commands will generate the necessary migration files and apply them to create the database, including the `Users` table with seeded data.
  
### Run the Application

1. **Run the Application**

  From the solution root folder, use the following command to start the application:

  ```bash
  dotnet run --project src/PackAndGo.Web
  ```

  After running this command, the application will start, and you can access it in your browser by navigating to `http://localhost:5000` (or the default URL provided in the terminal output).

### Conclusion

By following this comprehensive guide, we've gained valuable insights into building a web application using Clean Architecture principles. Specifically, the key learning outcomes include:

1. **Understanding Clean Architecture**: We've learned how to structure a web application into distinct layers—Domain, Application, Infrastructure, and Web (Presentation)—ensuring a clear separation of concerns and making our application scalable and maintainable.

2. **Domain-Driven Design (DDD)**: We've modeled core business entities, like the `User` entity, and now understand the importance of encapsulating business logic within the domain layer, which forms the core of our application.

3. **Implementing a Repository Pattern**: By implementing the repository pattern, we’ve abstracted data access, promoting a clean and testable design. We now know how to create and use repositories for CRUD operations effectively.

4. **Setting Up EF Core with SQLite**: We covered the installation and configuration of Entity Framework Core with SQLite, including running migrations and creating a database. This has equipped us with the skills to integrate a relational database into our applications.

5. **Building Application Services**: We've learned how to create application services that interact with the domain layer, handle business logic, and provide data to the presentation layer through data transfer objects (DTOs).

6. **Creating Minimalistic Views**: We created simple, functional Razor views in ASP.NET Core MVC, focusing on core functionality before styling. This approach emphasized the importance of getting the basics right first.

7. **Configuring Dependency Injection**: We've seen how to properly register services and repositories in the Dependency Injection (DI) container, which is crucial for maintaining a loosely coupled architecture.

8. **Launching and Testing the Application**: Finally, we ran the application and verified that all components—from the database to the user interface—are functioning correctly.

By completing this exercise, we have built a fully functional web application. This will give us a solid foundation when we improve this solution in the coming chapters.