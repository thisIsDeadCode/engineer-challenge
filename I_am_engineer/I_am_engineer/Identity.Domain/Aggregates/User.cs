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
        DateTimeOffset? passwordResetTokenExpiresAtUtc,
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

        if (passwordResetToken is null && passwordResetTokenExpiresAtUtc.HasValue)
        {
            throw new UserInvalidPasswordResetStateException();
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
        PasswordResetTokenExpiresAtUtc = passwordResetTokenExpiresAtUtc;
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
            passwordResetTokenExpiresAtUtc: null,
            createdAtUtc: now,
            updatedAtUtc: now)
    {
    }

    public Guid Id { get; }

    public Email Email { get; }

    public PasswordHash? PasswordHash { get; private set; }

    public bool IsPasswordSet => PasswordHash is not null;

    public LockoutPolicy LockoutPolicy { get; }

    public PasswordPolicy PasswordPolicy { get; }

    public bool IsActive { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public PasswordResetToken? PasswordResetToken { get; private set; }

    public DateTimeOffset? PasswordResetTokenExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static User Restore(
        Guid id,
        Email email,
        PasswordHash passwordHash,
        bool isActive,
        int failedLoginAttempts,
        DateTimeOffset? lockedUntilUtc,
        PasswordResetToken? passwordResetToken,
        DateTimeOffset? passwordResetTokenExpiresAtUtc,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        int lockoutMaxFailedAttempts,
        TimeSpan lockoutDuration)
    {
        var lockoutPolicy = new LockoutPolicy(lockoutMaxFailedAttempts, lockoutDuration);
        var passwordPolicy = new PasswordPolicy();

        return new User(
            id,
            email ?? throw new ArgumentNullException(nameof(email)),
            passwordHash ?? throw new ArgumentNullException(nameof(passwordHash)),
            lockoutPolicy,
            passwordPolicy,
            isActive,
            failedLoginAttempts,
            lockedUntilUtc,
            passwordResetToken,
            passwordResetTokenExpiresAtUtc,
            createdAtUtc,
            updatedAtUtc);
    }

    public static User RegisterNew(
        Guid id,
        Email email,
        DateTimeOffset now,
        int lockoutMaxFailedAttempts = LockoutPolicy.DefaultMaxFailedAttempts,
        TimeSpan? lockoutDuration = null)
    {
        var lockoutPolicy = new LockoutPolicy(lockoutMaxFailedAttempts, lockoutDuration ?? LockoutPolicy.DefaultLockoutDuration);
        var passwordPolicy = new PasswordPolicy();

        return new User(
            id,
            email ?? throw new ArgumentNullException(nameof(email)),
            lockoutPolicy,
            passwordPolicy,
            now);
    }

    public bool IsLockedOut(DateTimeOffset now)
    {
        return LockoutPolicy.IsLockedOut(LockedUntilUtc, now);
    }

    public void EnsurePasswordIsSet()
    {
        if (!IsPasswordSet)
        {
            throw new UserPasswordIsRequiredException();
        }
    }

    public void RecordFailedLoginAttempt(DateTimeOffset now)
    {
        ThrowIfInactive();
        EnsurePasswordIsSet();

        FailedLoginAttempts++;

        if (LockoutPolicy.ShouldLockout(FailedLoginAttempts))
        {
            LockedUntilUtc = LockoutPolicy.CalculateLockoutEnd(now);
        }

        UpdatedAtUtc = now;
    }

    public void RecordSuccessfulLogin(DateTimeOffset now)
    {
        ThrowIfInactive();
        ThrowIfLockedOut(now);
        EnsurePasswordIsSet();

        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
        UpdatedAtUtc = now;
    }

    public void SetPassword(IPasswordHasher passwordHasher, string newPassword, DateTimeOffset now)
    {
        ThrowIfInactive();

        ArgumentNullException.ThrowIfNull(passwordHasher);

        PasswordPolicy.EnsureCompliant(newPassword);

        var passwordHash = passwordHasher.Hash(newPassword);
        PasswordHash = new PasswordHash(passwordHash);

        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAtUtc = null;
        UpdatedAtUtc = now;
    }

    public void IssuePasswordResetToken(IPasswordResetTokenGenerator passwordResetTokenGenerator, DateTimeOffset expiresAtUtc, DateTimeOffset now)
    {
        ThrowIfInactive();
        EnsurePasswordIsSet();

        ArgumentNullException.ThrowIfNull(passwordResetTokenGenerator);

        if (expiresAtUtc <= now)
        {
            throw new UserInvalidPasswordResetExpirationException();
        }

        var generatedToken = passwordResetTokenGenerator.Generate();
        PasswordResetToken = new PasswordResetToken(generatedToken);
        PasswordResetTokenExpiresAtUtc = expiresAtUtc;
        UpdatedAtUtc = now;
    }

    public bool CanConfirmPasswordReset(PasswordResetToken providedToken, DateTimeOffset now)
    {
        EnsurePasswordIsSet();

        if (PasswordResetToken is null || PasswordResetTokenExpiresAtUtc is null)
        {
            return false;
        }

        return PasswordResetToken == providedToken && PasswordResetTokenExpiresAtUtc > now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        IsActive = false;
        UpdatedAtUtc = now;
    }

    private void ThrowIfInactive()
    {
        if (!IsActive)
        {
            throw new UserIsInactiveException();
        }
    }

    private void ThrowIfLockedOut(DateTimeOffset now)
    {
        if (IsLockedOut(now))
        {
            throw new UserIsLockedOutException();
        }
    }
}
