using PackAndGo.Domain.Common;

namespace PackAndGo.Domain.Exceptions;

public class EmptyEmailException : DomainException
{
    public EmptyEmailException()
        : base("The email address cannot be null or empty.")
    {
    }
}
