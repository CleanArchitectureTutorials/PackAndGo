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