## Project Setup

We will organize the application into four distinct project layers:

1. **Web (Presentation Layer)**: An ASP.NET Core MVC project that handles user interactions, such as HTTP requests and views.
2. **Application Layer**: This layer will contain business logic, orchestrating use cases and delegating tasks to the domain and infrastructure layers.
3. **Domain Layer**: The core of the application, where essential business entities and domain logic reside.
4. **Infrastructure Layer**: Responsible for data access, including the interaction with a SQLite database using Entity Framework Core.

Here’s a recommended directory structure for the **PackAndGo** solution. This structure follows the Clean Architecture principles with a clear separation of concerns and a well-organized project layout.

```plaintext
PackAndGo/
│
├── docs/                # Documentation and related resources
│   └── ...             # Files related to project documentation
│
├── src/                 # Source code projects
│   ├── PackAndGo.Web/          # ASP.NET Core Web project (UI Layer)
│   ├── PackAndGo.Application/  # Application Layer (Use Cases)
│   ├── PackAndGo.Domain/       # Domain Layer (Entities, Value Objects, etc)
│   └── PackAndGo.Infrastructure/ # Infrastructure Layer (Data Access)
│
└── tests/               # Test projects
    ├── ...
```

**Explanation:**

**`docs/`**:  

- This folder is for any documentation related to your project, including design decisions, architectural diagrams, API documentation, or user guides.

**`src/`**:

- **`PackAndGo.Web/`**: This project will host your ASP.NET Core MVC Web application. In our case it will be the presentation layer.
- **`PackAndGo.Application/`**: Contains the application layer, which includes business logic, use cases, services, and interfaces that the Web layer calls.
- **`PackAndGo.Domain/`**: This is the core of your application. It includes domain entities, aggregates, value objects, domain services, and domain interfaces.
- **`PackAndGo.Infrastructure/`**: Responsible for data access, third-party services, and any other external dependencies. It implements the interfaces defined in the Domain layer.

**`tests/`**:

- Each layer in your `src/` folder has a corresponding test project here.

### Create the Solution Structure

Start by creating a new solution for **PackAndGo** with four separate projects, each corresponding to one of the architecture layers. To maintain the clean architecture structure, we must also ensure that each project references the other projects correctly.

Create a directory called `PackAndGo`. Start a terminal in that directory and run the following commands. You can use VS Code and the integrated terminal to do these steps.

1. Create the directory structure

	```bash
	mkdir src tests docs
	```

2. Create the solution

	```bash
	dotnet new sln
	```

3. Create the projects and add them to the solution

	```bash
	dotnet new mvc -o src/PackAndGo.Web
	dotnet new classlib -o src/PackAndGo.Application
	dotnet new classlib -o src/PackAndGo.Domain
	dotnet new classlib -o src/PackAndGo.Infrastructure
	dotnet sln add src/PackAndGo.Web/PackAndGo.Web.csproj
	dotnet sln add src/PackAndGo.Application/PackAndGo.Application.csproj
	dotnet sln add src/PackAndGo.Domain/PackAndGo.Domain.csproj
	dotnet sln add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj
	```

4. Setup references between projects

	```bash
	dotnet add src/PackAndGo.Web/PackAndGo.Web.csproj reference src/PackAndGo.Application/PackAndGo.Application.csproj
	dotnet add src/PackAndGo.Web/PackAndGo.Web.csproj reference src/PackAndGo.Domain/PackAndGo.Domain.csproj
	dotnet add src/PackAndGo.Web/PackAndGo.Web.csproj reference src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj
	dotnet add src/PackAndGo.Application/PackAndGo.Application.csproj reference src/PackAndGo.Domain/PackAndGo.Domain.csproj
	dotnet add src/PackAndGo.Infrastructure/PackAndGo.Infrastructure.csproj reference src/PackAndGo.Domain/PackAndGo.Domain.csproj
	```

	> In a true clean architecture setup the Presentation layer (Web) shall only have a dependency to the Application layer. However, due to the fact that we must keep our _Dependency Injection Container_ somewhere we must choose where to have our _Composition Root_. In our case the Web project is our _Composition Root_, which means it must have dependencies to all other projects. We will later use static methods to limit the dependencies to the Application and Infrastructure layers, protecting at least the most important Domain layer.
	
5. Clean up example files

	There are som example files automatically generated in the class libraries called `Class1.cs`. These files can be safely removed with the following command:
		
	```bash
	find . -name Class1.cs -type f -delete
	```

6. Restore and build the solution

	Run the following commands and make sure you get no errors
	
	```bash
	dotnet restore
	dotnet build
	```

7. Run the Web app

	Run the Web application with the following command:
	
	```bash
	dotnet run --project src/PackAndGo.Web
	```
	
	Verify in a browser that you can start the application
	
	You can exit the program with `<CTRL> <C>`

8. Prepare for Git (optional)

	In order to only check in relevant files in a git repository you can create a `.gitignore` file with the following command:
	
	```bash
	dotnet new gitignore
	```
