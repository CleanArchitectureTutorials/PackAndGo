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