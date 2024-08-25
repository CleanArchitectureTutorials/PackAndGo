using PackAndGo.Application.DTOs;
using PackAndGo.Application.Interfaces;
using PackAndGo.Domain.Entities;
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
       var user = User.Create(userDto.Email);
       await _userRepository.AddAsync(user);
   }

   public async Task UpdateUserAsync(UserDTO userDto)
   {
        // Find the user by id
         var user = await _userRepository.GetByIdAsync(userDto.Id) ?? throw new Exception("User not found");
         
        // Update the user
        user.ChangeEmail(userDto.Email);
        await _userRepository.UpdateAsync(user);
    }

   public async Task DeleteUserAsync(Guid id)
   {
       await _userRepository.DeleteAsync(id);
   }
}
