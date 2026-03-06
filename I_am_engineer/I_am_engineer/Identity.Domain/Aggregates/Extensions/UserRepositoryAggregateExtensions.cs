using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.ValueObjects;

namespace I_am_engineer.Identity.Domain.Aggregates.Extensions;

public static class UserRepositoryAggregateExtensions
{
    public static async Task<User?> LoadUserAggregateByEmailAsync(
        this IUserRepository userRepository,
        string email,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userRepository);

        var userCredentials = await userRepository.GetUserCredentialsByEmailAsync(email, cancellationToken);
        if (userCredentials is null)
        {
            return null;
        }

        var resetToken = await userRepository.GetUserOneTimePasswordResetTokenAsync(userCredentials.UserId, cancellationToken);

        var passwordResetToken = resetToken is null || resetToken.IsUsed
            ? null
            : new PasswordResetToken(resetToken.ResetToken);

        var passwordResetTokenExpiresAtUtc = resetToken is null || resetToken.IsUsed
            ? null
            : resetToken.ExpiresAt;

        return User.Restore(
            id: userCredentials.UserId,
            email: new Email(userCredentials.Email),
            passwordHash: new PasswordHash(userCredentials.PasswordHash),
            isActive: userCredentials.IsActive,
            failedLoginAttempts: userCredentials.CurrentFailedAttempts,
            lockedUntilUtc: userCredentials.LockedUntil,
            passwordResetToken: passwordResetToken,
            passwordResetTokenExpiresAtUtc: passwordResetTokenExpiresAtUtc,
            createdAtUtc: userCredentials.CreatedAtUtc,
            updatedAtUtc: userCredentials.UpdatedAtUtc,
            lockoutMaxFailedAttempts: userCredentials.MaxFailedAttempts,
            lockoutDuration: TimeSpan.FromMinutes(userCredentials.LockoutDurationMinutes));
    }

    public static async Task<bool> SaveUserAggregateAsync(
        this IUserRepository userRepository,
        User user,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(user);

        user.EnsurePasswordIsSet();

        var existingUser = await userRepository.GetUserCredentialsByEmailAsync(user.Email.Value, cancellationToken);
        if (existingUser is null)
        {
            var userCreated = await userRepository.CreateUserAsync(
                user.Id,
                user.Email.Value,
                user.PasswordHash!.Value,
                cancellationToken);

            if (!userCreated)
            {
                return false;
            }
        }

        var lockoutSaved = await userRepository.UpdateUserLockoutAsync(
            user.Id,
            user.FailedLoginAttempts,
            user.LockedUntilUtc,
            cancellationToken);

        if (!lockoutSaved)
        {
            return false;
        }

        if (user.PasswordResetToken is null || user.PasswordResetTokenExpiresAtUtc is null)
        {
            return await userRepository.ClearUserOneTimePasswordResetTokenAsync(user.Id, cancellationToken);
        }

        return await userRepository.SaveUserOneTimePasswordResetTokenAsync(
            user.Id,
            user.PasswordResetToken.Value,
            user.PasswordResetTokenExpiresAtUtc.Value,
            cancellationToken);
    }
}
