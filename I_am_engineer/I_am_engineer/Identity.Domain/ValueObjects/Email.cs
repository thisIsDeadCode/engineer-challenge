using System.Text.RegularExpressions;

namespace I_am_engineer.Identity.Domain.ValueObjects;

public record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.");

        email = email.Trim();

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Incorrect email format.");

        return new Email(email);
    }

    public override string ToString() => Value;
}
