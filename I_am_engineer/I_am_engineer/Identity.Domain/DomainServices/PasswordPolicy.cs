using I_am_engineer.Identity.Domain.Exceptions.PasswordPolicy;

namespace I_am_engineer.Identity.Domain.DomainServices;

public sealed class PasswordPolicy
{
    public const int DefaultMinLength = 8;

    public const int DefaultMaxLength = 128;

    public int MinLength { get; }

    public int MaxLength { get; }

    public bool RequireUppercase { get; }

    public bool RequireLowercase { get; }

    public bool RequireDigit { get; }

    public bool RequireSpecialCharacter { get; }

    public PasswordPolicy()
        : this(DefaultMinLength, DefaultMaxLength, requireUppercase: true, requireLowercase: true, requireDigit: true, requireSpecialCharacter: true)
    {
    }

    public PasswordPolicy(
        int minLength,
        int maxLength,
        bool requireUppercase,
        bool requireLowercase,
        bool requireDigit,
        bool requireSpecialCharacter)
    {
        if (minLength <= 0)
        {
            throw new PasswordPolicyInvalidConfigurationException(nameof(minLength), "Minimum password length must be greater than zero.");
        }

        if (maxLength < minLength)
        {
            throw new PasswordPolicyInvalidConfigurationException(nameof(maxLength), "Maximum password length must be greater than or equal to minimum password length.");
        }

        if (!requireUppercase && !requireLowercase && !requireDigit && !requireSpecialCharacter)
        {
            throw new PasswordPolicyInvalidConfigurationException("characterRequirements", "At least one character category must be required.");
        }

        MinLength = minLength;
        MaxLength = maxLength;
        RequireUppercase = requireUppercase;
        RequireLowercase = requireLowercase;
        RequireDigit = requireDigit;
        RequireSpecialCharacter = requireSpecialCharacter;
    }

    public bool IsCompliant(string? password)
    {
        return Validate(password).Count == 0;
    }

    public IReadOnlyCollection<string> Validate(string? password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
            return errors;
        }

        if (password.Length < MinLength)
        {
            errors.Add($"Password must be at least {MinLength} characters long.");
        }

        if (password.Length > MaxLength)
        {
            errors.Add($"Password must be at most {MaxLength} characters long.");
        }

        if (RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase character.");
        }

        if (RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase character.");
        }

        if (RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit.");
        }

        if (RequireSpecialCharacter && password.All(char.IsLetterOrDigit))
        {
            errors.Add("Password must contain at least one special character.");
        }

        return errors;
    }

    public void EnsureCompliant(string? password)
    {
        var errors = Validate(password);
        if (errors.Count == 0)
        {
            return;
        }

        throw new PasswordPolicyViolationException(errors);
    }
}
