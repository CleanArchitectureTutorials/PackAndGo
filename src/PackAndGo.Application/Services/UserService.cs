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
