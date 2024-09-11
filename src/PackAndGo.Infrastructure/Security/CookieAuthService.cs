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
