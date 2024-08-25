using PackAndGo.Domain.Common;
using PackAndGo.Domain.ValueObjects;

namespace PackAndGo.Domain.Entities;

public class User : Entity
{
    public Email Email { get; private set; }

    private User(Guid id, Email email)
    {
        Id = id;
        Email = email;
    }

    public static User Create(string email)
    {
        return new User(Guid.NewGuid(), new Email(email));
    }

    public static User Load(Guid id, string email)
    {
        return new User(id, new Email(email));
    }

    public void ChangeEmail(string email)
    {
        Email = new Email(email);
    }
}