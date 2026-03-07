using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.Events;
using I_am_engineer.Identity.Domain.Events.User;
using I_am_engineer.Identity.Domain.Exceptions.User;
using I_am_engineer.Identity.Domain.ValueObjects;

namespace I_am_engineer.Identity.Domain.Aggregates;

public sealed class User : IEventEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private User(
        Guid id,
        Email email,
        PasswordHash? passwordHash,
        Session? session,
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
        DateTimeOffset now)
        : this(
            id,
            email,
            passwordHash: null,
            session: null,
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
        DateTimeOffset? passwordResetTokenIssuedAt,
        int failedLoginAttempts,
        DateTimeOffset? lockedUntilUtc,
        bool isActive,
        Session? session,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        return new User(
            id,
            Email.Create(email),
            new PasswordHash(passwordHash),
            session,
            isActive,
            failedLoginAttempts,
            lockedUntilUtc,
            passwordResetTokenValue is null ? null : PasswordResetToken.Create(passwordResetTokenValue, passwordResetTokenIsUsed, passwordResetTokenExpiresAt, passwordResetTokenIssuedAt),
            createdAtUtc,
            updatedAtUtc);
    }

    public static User CreateNew(string email, string password, IPasswordHasher passwordHasher, PasswordPolicy passwordPolicy)
    {
        var user = new User(
            Guid.NewGuid(),
            Email.Create(email),
            DateTimeOffset.UtcNow);

        user.SetPassword(passwordHasher, passwordPolicy, password);

        user.AddDomainEvent(new UserRegistered(user.Id));

        user.IsChanged = true;

        return user;
    }

    public void RecordFailedLoginAttempt(LockoutPolicy lockoutPolicy)
    {
        ThrowIfInactive();

        var now = DateTimeOffset.UtcNow;
        FailedLoginAttempts++;

        if (lockoutPolicy.ShouldLockout(FailedLoginAttempts))
        {
            LockedUntilUtc = lockoutPolicy.CalculateLockoutEnd(now);
            AddDomainEvent(new UserLockedOut(Id));
        }

        UpdatedAtUtc = now;
        IsChanged = true;
    }

    public void RecordSuccessfulLogin(LockoutPolicy lockoutPolicy)
    {
        ThrowIfInactive();
        ThrowIfLockedOut(lockoutPolicy);

        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
    }

    public void SetPassword(IPasswordHasher passwordHasher, PasswordPolicy passwordPolicy, string newPassword)
    {
        ThrowIfInactive();

        ArgumentNullException.ThrowIfNull(passwordHasher);

        passwordPolicy.EnsureCompliant(newPassword);

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
        AddDomainEvent(new PasswordResetRequested(Id));
    }

    public void MarkPasswordResetTokenAsUsed(string providedToken, PasswordRecoveryPolicy passwordRecoveryPolicy)
    {
        ThrowIfInactive();

        if (!CanConfirmPasswordReset(providedToken, passwordRecoveryPolicy))
        {
            throw new InvalidOperationException("Password reset token is invalid or expired.");
        }

        PasswordResetToken = I_am_engineer.Identity.Domain.ValueObjects.PasswordResetToken.Create(
            PasswordResetToken!.Value,
            isUsed: true,
            PasswordResetToken.ExpiresAt,
            PasswordResetToken.IssuedAt);

        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;
        AddDomainEvent(new PasswordResetTokenUsed(Id));
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
        AddDomainEvent(new UserDeactivated(Id));
    }


    public bool CanConfirmPasswordReset(string providedToken, PasswordRecoveryPolicy passwordRecoveryPolicy)
    {
        ArgumentNullException.ThrowIfNull(passwordRecoveryPolicy);

        if (PasswordResetToken is null || !passwordRecoveryPolicy.IsTokenActive(PasswordResetToken, DateTimeOffset.UtcNow))
        {
            return false;
        }

        return PasswordResetToken.Value == providedToken;
    }

    public bool IsLockedOut(LockoutPolicy lockoutPolicy)
    {
        return lockoutPolicy.IsLockedOut(LockedUntilUtc, DateTimeOffset.UtcNow);
    }

    private void ThrowIfInactive()
    {
        if (!IsActive)
        {
            throw new UserIsInactiveException();
        }
    }

    private void ThrowIfLockedOut(LockoutPolicy lockoutPolicy)
    {
        if (IsLockedOut(lockoutPolicy))
        {
            throw new UserIsLockedOutException();
        }
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
