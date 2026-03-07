namespace I_am_engineer.Identity.Domain.ValueObjects;

public record PasswordResetToken
{
    public string Value { get; }
    public bool IsUsed { get; }
    public DateTimeOffset ExpiresAt { get; }
    public bool IsActive => IsUsed ? false : DateTimeOffset.UtcNow < ExpiresAt;

    private PasswordResetToken(string value, bool isUsed, DateTimeOffset expiresAt)
    {
        Value = value;
        IsUsed = isUsed;
        ExpiresAt = expiresAt;
    }

    public static PasswordResetToken Create(string? value, bool? isUsed, DateTimeOffset? expiresAt)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PasswordResetToken cannot be null or empty.");

        if (!expiresAt.HasValue)
            throw new ArgumentException("ExpiresAt cannot be null.");

        return new PasswordResetToken(value, isUsed ?? false, expiresAt.Value);
    }

    public override string ToString() => Value;
}