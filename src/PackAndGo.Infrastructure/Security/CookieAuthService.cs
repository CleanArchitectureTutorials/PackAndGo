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
