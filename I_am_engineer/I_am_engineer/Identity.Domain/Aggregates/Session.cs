using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.Events;

namespace I_am_engineer.Identity.Domain.Aggregates;

public sealed class Session
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private Session(
        Guid id,
        Guid userId,
        string refreshToken,
        DateTimeOffset refreshTokenExpiresAt,
        string? deviceId,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Session id is required.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.", nameof(refreshToken));
        }

        if (refreshTokenExpiresAt <= createdAtUtc)
        {
            throw new ArgumentException("Refresh token expiration must be greater than creation date.", nameof(refreshTokenExpiresAt));
        }

        Id = id;
        UserId = userId;
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        DeviceId = deviceId;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public string RefreshToken { get; private set; }

    public DateTimeOffset RefreshTokenExpiresAt { get; private set; }

    public string? DeviceId { get; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public bool IsChanged { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Session Open(Guid userId, string? deviceId, ITokenGenerator tokenGenerator, TimeSpan refreshTokenLifetime)
    {
        ArgumentNullException.ThrowIfNull(tokenGenerator);

        if (refreshTokenLifetime <= TimeSpan.Zero)
        {
            throw new ArgumentException("Refresh token lifetime must be positive.", nameof(refreshTokenLifetime));
        }

        var now = DateTimeOffset.UtcNow;
        var refreshToken = tokenGenerator.GenerateRefreshToken();
        var session = new Session(
            id: Guid.NewGuid(),
            userId: userId,
            refreshToken: refreshToken.Value,
            refreshTokenExpiresAt: now.Add(refreshTokenLifetime),
            deviceId: deviceId,
            isActive: true,
            createdAtUtc: now,
            updatedAtUtc: now)
        {
            IsChanged = true
        };

        session.AddDomainEvent(new SessionOpened(session.Id, session.UserId, session.RefreshTokenExpiresAt));

        return session;
    }

    public static Session Restore(
        Guid id,
        Guid userId,
        string refreshToken,
        DateTimeOffset refreshTokenExpiresAt,
        string? deviceId,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        return new Session(
            id,
            userId,
            refreshToken,
            refreshTokenExpiresAt,
            deviceId,
            isActive,
            createdAtUtc,
            updatedAtUtc);
    }

    public void Rotate(ITokenGenerator tokenGenerator, TimeSpan refreshTokenLifetime)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot rotate inactive session.");
        }

        ArgumentNullException.ThrowIfNull(tokenGenerator);

        if (refreshTokenLifetime <= TimeSpan.Zero)
        {
            throw new ArgumentException("Refresh token lifetime must be positive.", nameof(refreshTokenLifetime));
        }

        var nextRefreshToken = tokenGenerator.GenerateRefreshToken();

        RefreshToken = nextRefreshToken.Value;
        RefreshTokenExpiresAt = DateTimeOffset.UtcNow.Add(refreshTokenLifetime);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;

        AddDomainEvent(new SessionRotated(Id, UserId, RefreshTokenExpiresAt));
    }

    public void Revoke()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
        IsChanged = true;

        AddDomainEvent(new SessionRevoked(Id, UserId));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
