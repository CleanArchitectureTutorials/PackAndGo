namespace PackAndGo.Application.Interfaces;

public interface IAuthService
{
    Task RegisterUserAsync(string email);
    Task SignInAsync(string email);
    Task SignOutAsync();
}
