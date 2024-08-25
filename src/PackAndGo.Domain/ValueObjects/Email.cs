using System.Text.RegularExpressions;
using PackAndGo.Domain.Common;
using PackAndGo.Domain.Exceptions;

namespace PackAndGo.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new EmptyEmailException();

        if (!IsValidEmail(email))
            throw new InvalidEmailException(email);

        Value = email;
    }

    private static bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailRegex);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}