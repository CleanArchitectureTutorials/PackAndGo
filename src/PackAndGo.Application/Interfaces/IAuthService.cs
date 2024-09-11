namespace PackAndGo.Application.Interfaces;

public interface IAuthService
{
    Task SignInAsync(string email);
    Task SignOutAsync();
}
