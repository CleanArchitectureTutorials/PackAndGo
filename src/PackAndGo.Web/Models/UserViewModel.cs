using System.ComponentModel.DataAnnotations;

namespace PackAndGo.Web.Models;

public class UserViewModel
{
   public Guid Id { get; set; }
   
   [Required(ErrorMessage = "Email is required.")]
   [EmailAddress(ErrorMessage = "Invalid email address.")]
   public string Email { get; set; } = string.Empty;
}