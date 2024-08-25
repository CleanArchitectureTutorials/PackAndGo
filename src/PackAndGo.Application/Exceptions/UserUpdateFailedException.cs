namespace PackAndGo.Application.Exceptions;

public class UserUpdateFailedException : Exception
{
    public UserUpdateFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}