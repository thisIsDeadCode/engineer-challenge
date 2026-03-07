using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.Exceptions.User;
using I_am_engineer.Identity.Domain.ValueObjects;

namespace I_am_engineer.Identity.Domain.Aggregates;

public sealed class User
{
    private User(
        Guid id,
        Email email,
        PasswordHash? passwordHash,
        LockoutPolicy lockoutPolicy,
        PasswordPolicy passwordPolicy,
        bool isActive,
        int failedLoginAttempts,
        DateTimeOffset? lockedUntilUtc,
        PasswordResetToken? passwordResetToken,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new UserIdIsRequiredException();
        }

        if (failedLoginAttempts < 0)
        {
            throw new UserInvalidFailedLoginAttemptsException(failedLoginAttempts);
        }

        if (lockedUntilUtc.HasValue && lockedUntilUtc.Value < createdAtUtc)
        {
            throw new UserInvalidLockoutTimestampException();
        }

        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        LockoutPolicy = lockoutPolicy ?? throw new ArgumentNullException(nameof(lockoutPolicy));
        PasswordPolicy = passwordPolicy ?? throw new ArgumentNullException(nameof(passwordPolicy));
        IsActive = isActive;
        FailedLoginAttempts = failedLoginAttempts;
        LockedUntilUtc = lockedUntilUtc;
        PasswordResetToken = passwordResetToken;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }


    private User(
        Guid id,
        Email email,
        LockoutPolicy lockoutPolicy,
        PasswordPolicy passwordPolicy,
        DateTimeOffset now)
        : this(
            id,
            email,
            passwordHash: null,
            lockoutPolicy,
            passwordPolicy,
            isActive: true,
            failedLoginAttempts: 0,
            lockedUntilUtc: null,
            passwordResetToken: null,
            createdAtUtc: now,
            updatedAtUtc: now)
    {
    }

    public Guid Id { get; }

    public Email Email { get; }

    public PasswordHash? PasswordHash { get; private set; }

    public PasswordResetToken? PasswordResetToken { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public bool IsActive { get; private set; }

    public LockoutPolicy LockoutPolicy { get; }

    public PasswordPolicy PasswordPolicy { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }


    public bool IsPasswordSet => PasswordHash is not null;
    public bool IsChanged { get; private set; }


    public static User Restore(
        Guid id,
        string email,
        string passwordHash,
        string? passwordResetTokenValue,
        bool? passwordResetTokenIsUsed,
        DateTimeOffset? passwordResetTokenExpiresAt,
        int failedLoginAttempts,
        DateTimeOffset? lockedUntilUtc,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        var lockoutPolicy = new LockoutPolicy();
        var passwordPolicy = new PasswordPolicy();

        return new User(
            id,
            Email.Create(email),
            new PasswordHash(passwordHash),
            lockoutPolicy,
            passwordPolicy,
            isActive,
            failedLoginAttempts,
            lockedUntilUtc,
            passwordResetTokenValue is null ? null : PasswordResetToken.Create(passwordResetTokenValue, passwordResetTokenIsUsed, passwordResetTokenExpiresAt),
            createdAtUtc,
            updatedAtUtc);
    }

    public static User CreateNew(string email)
    {
        var lockoutPolicy = new LockoutPolicy();
        var passwordPolicy = new PasswordPolicy();

        var user = new User(
            Guid.NewGuid(),
            Email.Create(email),
            lockoutPolicy,
            passwordPolicy,
            DateTimeOffset.UtcNow);

        user.IsChanged = true;

        return user;
    }

    public void RecordFailedLoginAttempt()
    {
        ThrowIfInactive();
        EnsurePasswordIsSet();

        var now = DateTimeOffset.UtcNow;
        FailedLoginAttempts++;

        if (LockoutPolicy.ShouldLockout(FailedLoginAttempts))
        {
            LockedUntilUtc = LockoutPolicy.CalculateLockoutEnd(now);
        }

        UpdatedAtUtc = now;
        IsChanged = true;
    }

    public void RecordSuccessfulLogin()
    {
        ThrowIfInactive();
        ThrowIfLockedOut();
        EnsurePasswordIsSet();

        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
    }

    public void SetPassword(IPasswordHasher passwordHasher, string newPassword)
    {
        ThrowIfInactive();

        ArgumentNullException.ThrowIfNull(passwordHasher);

        PasswordPolicy.EnsureCompliant(newPassword);

        PasswordHash = passwordHasher.Hash(newPassword);

        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
        PasswordResetToken = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
    }

    public void IssuePasswordResetToken(IPasswordResetTokenGenerator passwordResetTokenGenerator)
    {
        ThrowIfInactive();
        EnsurePasswordIsSet();

        ArgumentNullException.ThrowIfNull(passwordResetTokenGenerator);

        PasswordResetToken = passwordResetTokenGenerator.GenerateToken();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
    }

    public void EnsurePasswordIsSet()
    {
        if (!IsPasswordSet)
        {
            throw new UserPasswordIsRequiredException();
        }
    }

    public bool CanConfirmPasswordReset(string providedToken, DateTimeOffset now)
    {
        EnsurePasswordIsSet();

        if (PasswordResetToken is null || !PasswordResetToken.IsActive)
        {
            return false;
        }

        return PasswordResetToken.Value == providedToken;
    }

    public bool IsLockedOut()
    {
        return LockoutPolicy.IsLockedOut(LockedUntilUtc, DateTimeOffset.UtcNow);
    }

    private void ThrowIfInactive()
    {
        if (!IsActive)
        {
            throw new UserIsInactiveException();
        }
    }

    private void ThrowIfLockedOut()
    {
        if (IsLockedOut())
        {
            throw new UserIsLockedOutException();
        }
    }
}
