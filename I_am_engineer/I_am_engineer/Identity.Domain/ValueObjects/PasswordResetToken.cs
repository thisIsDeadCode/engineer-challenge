namespace I_am_engineer.Identity.Domain.ValueObjects;

public record PasswordResetToken
{
    public string Value { get; }
    public bool IsUsed { get; }
    public DateTimeOffset IssuedAt { get; }
    public DateTimeOffset ExpiresAt { get; }
    public bool IsActive => !IsUsed && DateTimeOffset.UtcNow < ExpiresAt;

    private PasswordResetToken(string value, bool isUsed, DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        Value = value;
        IsUsed = isUsed;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
    }

    public static PasswordResetToken Create(string? value, bool? isUsed, DateTimeOffset? expiresAt, DateTimeOffset? issuedAt = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PasswordResetToken cannot be null or empty.");

        if (!expiresAt.HasValue)
            throw new ArgumentException("ExpiresAt cannot be null.");

        var issuedAtValue = issuedAt ?? DateTimeOffset.UtcNow;

        if (issuedAtValue > expiresAt.Value)
            throw new ArgumentException("IssuedAt cannot be greater than ExpiresAt.");

        return new PasswordResetToken(value, isUsed ?? false, issuedAtValue, expiresAt.Value);
    }

    public override string ToString() => Value;
}
