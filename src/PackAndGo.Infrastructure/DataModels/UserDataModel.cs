using PackAndGo.Domain.Entities;

namespace PackAndGo.Infrastructure.DataModels;

public class UserDataModel
{
   public Guid Id { get; set; }
   public string Email { get; set; } = string.Empty;

   public User ToDomain()
   {
      return User.Load(Id, Email);
   }

   public static UserDataModel FromDomain(User user)
   {
      return new UserDataModel
      {
         Id = user.Id,
         Email = user.Email.Value
      };
   }

}