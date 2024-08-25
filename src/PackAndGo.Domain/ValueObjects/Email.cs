using System.Text.RegularExpressions;
using PackAndGo.Domain.Common;

namespace PackAndGo.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    public Email(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Email address is invalid", nameof(email));

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