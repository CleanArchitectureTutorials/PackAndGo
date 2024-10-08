using PackAndGo.Domain.Entities;

namespace PackAndGo.Domain.Repositories;

public interface IUserRepository
{
   Task<User?> GetByIdAsync(Guid id);
   Task<IEnumerable<User>> GetAllAsync();
   Task AddAsync(User user);
   Task UpdateAsync(User user);
   Task DeleteAsync(Guid id);
}
