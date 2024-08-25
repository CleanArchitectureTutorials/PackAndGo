using PackAndGo.Domain.Common;

namespace PackAndGo.Domain.Exceptions;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string email)
        : base($"The email '{email}' is not valid.")
    {
    }
}
