using Microsoft.EntityFrameworkCore;
using PackAndGo.Domain.Entities;
using PackAndGo.Domain.Repositories;
using PackAndGo.Infrastructure.DataModels;
using PackAndGo.Infrastructure.Persistence;

namespace PackAndGo.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
   private readonly AppDbContext _context;

   public UserRepository(AppDbContext context)
   {
       _context = context;
   }

   public async Task<User?> GetByIdAsync(Guid id)
   {
       var dataModel = await _context.Users.FindAsync(id);
       return dataModel == null ? null : dataModel.ToDomain();
   }

   public async Task<IEnumerable<User>> GetAllAsync()
   {
        return await _context.Users
            .Select(u => u.ToDomain())
            .ToListAsync();
   }

   public async Task AddAsync(User user)
   {
       var dataModel = UserDataModel.FromDomain(user);
       _context.Users.Add(dataModel);
       await _context.SaveChangesAsync();
   }

   public async Task UpdateAsync(User user)
   {
        var dataModel = await _context.Users.FindAsync(user.Id);
        if (dataModel != null)
        {
            dataModel.Email = user.Email.Value;
            await _context.SaveChangesAsync();
        }
   }

   public async Task DeleteAsync(Guid id)
   {
       var dataModel = await _context.Users.FindAsync(id);
       if (dataModel != null)
       {
           _context.Users.Remove(dataModel);
           await _context.SaveChangesAsync();
       }
   }
}
