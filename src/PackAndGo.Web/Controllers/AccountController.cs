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
