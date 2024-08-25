## Improve Error Handling with Custom Exceptions

To improve the robustness and user experience of your application, we can implement custom exceptions in the domain layer and ensuring these exceptions are appropriately handled and communicated through all layers. By taking the `Email` value object as an example, we will demonstrate how to structure and propagate errors effectively.

### Create Custom Exceptions in the Domain Layer

Let´s create custom exceptions that represent specific domain errors. These exceptions will encapsulate error details and ensure that invalid operations in the domain model are communicated clearly.

- **Define a Base Domain Exception**:
	  Create a base exception class for domain-related errors. This class will serve as the parent for all domain-specific exceptions.
	
	```csharp
	namespace PackAndGo.Domain.Common;
		
	public class DomainException : Exception
	{
	  public DomainException(string message) : base(message) { }
	}
	```

- **Create Specific Custom Exceptions**:
	  Define custom exceptions for `Email` validation errors. This exception will be thrown whenever an invalid email is encountered.
		
	```csharp
	using PackAndGo.Domain.Common;
	
	namespace PackAndGo.Domain.Exceptions;
	
	public class InvalidEmailException : DomainException
	{
		public InvalidEmailException(string email)
		    : base($"The email '{email}' is not valid.")
		{
		}
	}
	```
	
	```csharp
	using PackAndGo.Domain.Common;
	
	namespace PackAndGo.Domain.Exceptions;
	
	public class EmptyEmailException : DomainException
	{
	    public EmptyEmailException()
	        : base("The email address cannot be null or empty.")
	    {
	    }
	}
	```

- **Refactor the Email Value Object**:
  Update the `Email` class to throw custom exceptions instead of generic exceptions.
	
  	```csharp
	using System.Text.RegularExpressions;
	using PackAndGo.Domain.Common;
	using PackAndGo.Domain.Exceptions;
	
	namespace PackAndGo.Domain.ValueObjects;
	
	public class Email : ValueObject
	{
	    public string Value { get; }
	
	    public Email(string email)
	    {
	        if (string.IsNullOrWhiteSpace(email))
	            throw new EmptyEmailException();
	
	        if (!IsValidEmail(email))
	            throw new InvalidEmailException(email);
	
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
	Here is an example of how the exception lokks like before and after
	
	**Before**
	
	An unhandled exception occurred while processing the request.
	
	ArgumentException: Email address is invalid (Parameter 'email')
	PackAndGo.Domain.ValueObjects.Email..ctor(string email) in Email.cs, line 16
	
	**After**
	
	An unhandled exception occurred while processing the request.
	
	InvalidEmailException: The email 'asdsd' is not valid.
	PackAndGo.Domain.ValueObjects.Email..ctor(string email) in Email.cs, line 17
	


### Handling Custom Exceptions in the Application Layer

The application layer should catch domain-specific exceptions and either propagate them or translate them into application-specific errors.

- **Create Application-Specific Exceptions**:
  Define exceptions in the application layer that represent errors occurring during operations like user creation.

  ```csharp
  namespace PackAndGo.Application.Exceptions;

  public class UserCreationFailedException : Exception
  {
      public UserCreationFailedException(string message, Exception innerException)
          : base(message, innerException) { }
  }
  ```

  ```csharp
	namespace PackAndGo.Application.Exceptions;
	
	public class UserUpdateFailedException : Exception
	{
	    public UserUpdateFailedException(string message, Exception innerException)
	        : base(message, innerException) { }
	}
  ```

- **Wrap Domain Operations**:
  In the `UserService`, handle domain exceptions and decide whether to propagate them or translate them into application-level errors.

  	```csharp
	using PackAndGo.Application.DTOs;
	using PackAndGo.Application.Exceptions;
	using PackAndGo.Application.Interfaces;
	using PackAndGo.Domain.Entities;
	using PackAndGo.Domain.Exceptions;
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
	        try
	        {
	            var user = User.Create(userDto.Email);
	            await _userRepository.AddAsync(user);
	        }
	        catch (Exception ex) when (ex is InvalidEmailException || ex is EmptyEmailException)
	        {
	            throw new UserCreationFailedException("User creation failed due to invalid input.", ex);
	        }
	        catch (Exception ex)
	        {
	            throw new UserCreationFailedException("An unexpected error occurred while creating the user.", ex);
	        }
	   }
	
	   public async Task UpdateUserAsync(UserDTO userDto)
	   {
	        try
	        {
	            // Find the user by id
	            var user = await _userRepository.GetByIdAsync(userDto.Id);
	            if (user == null)
	            {
	                throw new UserUpdateFailedException($"User with ID '{userDto.Id}' not found.", null);
	            }
	
	            // Update the user
	            user.ChangeEmail(userDto.Email);
	            await _userRepository.UpdateAsync(user);
	        }
	        catch (Exception ex) when (ex is InvalidEmailException || ex is EmptyEmailException)
	        {
	            throw new UserUpdateFailedException("User update failed due to invalid input.", ex);
	        }
	        catch (Exception ex)
	        {
	            throw new UserUpdateFailedException("An unexpected error occurred while updating the user.", ex);
	        }
	    }
	
	   public async Task DeleteUserAsync(Guid id)
	   {
	       await _userRepository.DeleteAsync(id);
	   }
	}
  	```

	**Key Changes and Additions**
	
	- Custom Exception Handling for Creation and Update:
	AddUserAsync and UpdateUserAsync methods now catch EmptyEmailException and InvalidEmailException and translate them into UserCreationFailedException and UserUpdateFailedException, respectively.
	
	- Generic Exception Handling: If any other unexpected exceptions occur, they're caught and rethrown as more specific exceptions related to user creation or update.
	Error Messages:

### Communicating Errors in the Presentation Layer

The presentation layer should handle application exceptions and provide meaningful feedback to the user through error messages on the UI.

- **Handle Exceptions in Controllers**:
  In the `UserController`, catch application exceptions and return appropriate responses or error views.

  	```csharp
	using Microsoft.AspNetCore.Mvc;
	using PackAndGo.Application.DTOs;
	using PackAndGo.Application.Exceptions;
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
	            try
	            {
	                var userDto = new UserDTO { Id = Guid.NewGuid(), Email = model.Email };
	                await _userService.AddUserAsync(userDto);
	                return RedirectToAction(nameof(Index));
	            }
	            catch (UserCreationFailedException)
	            {
	                ModelState.AddModelError("", "There was an issue creating the user. Please try again.");
	            }
	            catch (Exception)
	            {
	                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
	            }
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
	            try
	            {
	                var userDto = new UserDTO { Id = model.Id, Email = model.Email };
	                await _userService.UpdateUserAsync(userDto);
	                return RedirectToAction(nameof(Index));
	            }
	            catch (UserUpdateFailedException)
	            {
	                ModelState.AddModelError("", "There was an issue updating the user. Please try again.");
	            }
	            catch (Exception)
	            {
	                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
	            }
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

- **Display Error Messages in Views**:
  In your Razor views, display the error messages captured in the `ModelState`. Add this code snippet to the `Create` and `Edit` views:

  ```html
  @if (!ViewData.ModelState.IsValid)
  {
      <div class="alert alert-danger">
          @foreach (var error in ViewData.ModelState.Values.SelectMany(v => v.Errors))
          {
              <p>@error.ErrorMessage</p>
          }
      </div>
  }
  ```


	**Explanation:**

	- **Inner Exception Handling**: When an application-level exception like `UserCreationFailedException` is thrown, it includes the domain-level exception (e.g., `InvalidEmailException` or `EmptyEmailException`) as its inner exception.
	- **Root Cause Extraction**: In the controller, you check if there’s an `InnerException` and extract its message. This message is then added to the `ModelState` and displayed to the user, providing insight into what went wrong.
	- **Fallback Message**: If there’s no inner exception (which could happen in general exceptions), a generic message is shown instead.


### Summary

By introducing custom exceptions in the domain layer, we can make error handling more expressive and meaningful. These exceptions can then be propagated through the application and presentation layers, where they are handled and communicated to the user in a clear and user-friendly manner. The key takeaways are:

1. **Custom Domain Exceptions**: Encapsulate domain-specific errors within custom exceptions that clearly communicate what went wrong.
2. **Application Layer Handling**: Catch domain exceptions, translate them as needed, and propagate them up the stack.
3. **User Feedback**: Ensure that errors are communicated to users in a clear and actionable way, improving the user experience.


This approach improves error handling and enhances the robustness and maintainability of the application, providing a better experience for both developers and users.

