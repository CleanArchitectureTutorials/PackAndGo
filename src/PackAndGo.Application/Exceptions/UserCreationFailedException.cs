namespace PackAndGo.Application.Exceptions;

public class UserCreationFailedException : Exception
{
  public UserCreationFailedException(string message, Exception innerException)
      : base(message, innerException) { }
}