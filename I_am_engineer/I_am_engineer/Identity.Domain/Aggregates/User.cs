using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.Events;
using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.Exceptions.User;
using I_am_engineer.Identity.Domain.ValueObjects;

namespace I_am_engineer.Identity.Domain.Aggregates;

public sealed class User
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private User(
        Guid id,
        Email email,
        PasswordHash? passwordHash,
        Session? session,
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
        Session = session;
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
            session: null,
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

    public Session? Session { get; private set; }

    public PasswordResetToken? PasswordResetToken { get; private set; }

    public int FailedLoginAttempts { get; private set; }

    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public bool IsActive { get; private set; }

    public LockoutPolicy LockoutPolicy { get; }

    public PasswordPolicy PasswordPolicy { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public bool IsChanged { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


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
        Session? session,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        var lockoutPolicy = new LockoutPolicy();
        var passwordPolicy = new PasswordPolicy();

        return new User(
            id,
            Email.Create(email),
            new PasswordHash(passwordHash),
            session,
            lockoutPolicy,
            passwordPolicy,
            isActive,
            failedLoginAttempts,
            lockedUntilUtc,
            passwordResetTokenValue is null ? null : PasswordResetToken.Create(passwordResetTokenValue, passwordResetTokenIsUsed, passwordResetTokenExpiresAt),
            createdAtUtc,
            updatedAtUtc);
    }

    public static User CreateNew(string email, string password, IPasswordHasher passwordHasher)
    {
        var lockoutPolicy = new LockoutPolicy();
        var passwordPolicy = new PasswordPolicy();

        var user = new User(
            Guid.NewGuid(),
            Email.Create(email),
            lockoutPolicy,
            passwordPolicy,
            DateTimeOffset.UtcNow);

        user.SetPassword(passwordHasher, password);

        user.AddDomainEvent(new UserRegistered(user.Id, user.Email.Value));

        user.IsChanged = true;

        return user;
    }

    public void RecordFailedLoginAttempt()
    {
        ThrowIfInactive();

        var now = DateTimeOffset.UtcNow;
        FailedLoginAttempts++;

        if (LockoutPolicy.ShouldLockout(FailedLoginAttempts))
        {
            LockedUntilUtc = LockoutPolicy.CalculateLockoutEnd(now);
            AddDomainEvent(new UserLockedOut(Id, LockedUntilUtc));
        }

        UpdatedAtUtc = now;
        IsChanged = true;
    }

    public void RecordSuccessfulLogin()
    {
        ThrowIfInactive();
        ThrowIfLockedOut();

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
        AddDomainEvent(new PasswordChanged(Id));
    }

    public void IssuePasswordResetToken(IPasswordResetTokenGenerator passwordResetTokenGenerator)
    {
        ThrowIfInactive();

        ArgumentNullException.ThrowIfNull(passwordResetTokenGenerator);

        PasswordResetToken = passwordResetTokenGenerator.GenerateToken();
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
        AddDomainEvent(new PasswordResetRequested(Id, PasswordResetToken.Value, PasswordResetToken.ExpiresAt));
    }

    public void MarkPasswordResetTokenAsUsed(string providedToken)
    {
        ThrowIfInactive();

        if (!CanConfirmPasswordReset(providedToken))
        {
            throw new InvalidOperationException("Password reset token is invalid or expired.");
        }

        PasswordResetToken = I_am_engineer.Identity.Domain.ValueObjects.PasswordResetToken.Create(
            PasswordResetToken!.Value,
            isUsed: true,
            PasswordResetToken.ExpiresAt);

        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
    }

    public bool CanConfirmPasswordReset(string providedToken)
    {
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

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
