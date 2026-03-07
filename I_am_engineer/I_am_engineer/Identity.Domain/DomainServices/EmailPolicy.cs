namespace I_am_engineer.Identity.Domain.DomainServices;

public sealed class EmailPolicy
{
    public string Normalize(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        var trimmed = email.Trim();
        if (!Validate(trimmed))
        {
            throw new ArgumentException("Incorrect email format.", nameof(email));
        }

        return trimmed.ToLowerInvariant();
    }

    public bool Validate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var value = email.Trim();
        var separatorIndex = value.LastIndexOf('@');

        if (separatorIndex <= 0 || separatorIndex != value.IndexOf('@') || separatorIndex == value.Length - 1)
        {
            return false;
        }

        var localPart = value[..separatorIndex];
        var domainPart = value[(separatorIndex + 1)..];

        if (localPart.Any(char.IsWhiteSpace) || domainPart.Any(char.IsWhiteSpace))
        {
            return false;
        }

        if (!domainPart.Contains('.') || domainPart.StartsWith('.') || domainPart.EndsWith('.'))
        {
            return false;
        }

        return true;
    }
}
