using I_am_engineer.Identity.Application.Abstractions;

namespace I_am_engineer.Identity.Domain.Aggregates.Extensions;

public static class UserRepositoryAggregateExtensions
{
    public static async Task<User?> RestoreUserAggregateByEmailAsync(
        this IUserRepository userRepository,
        string email,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userRepository);

        var userCredentials = await userRepository.GetUserCredentialsByEmailAsync(email, cancellationToken);
        if (userCredentials is null)
        {
            throw new ArgumentException("Can not restore user. UserCredentials is null.");
        }

        var resetToken = await userRepository.GetUserOneTimePasswordResetTokenAsync(userCredentials.UserId, cancellationToken);

        return User.Restore(
            id: userCredentials.UserId,
            email: userCredentials.Email,
            passwordHash: userCredentials.PasswordHash,
            passwordResetTokenValue: resetToken?.ResetToken,
            passwordResetTokenIsUsed: resetToken?.IsUsed,
            passwordResetTokenExpiresAt: resetToken?.ExpiresAt,
            failedLoginAttempts: userCredentials.CurrentFailedAttempts,
            lockedUntilUtc: userCredentials.LockedUntil,
            isActive: userCredentials.IsActive,
            createdAtUtc: userCredentials.CreatedAtUtc,
            updatedAtUtc: userCredentials.UpdatedAtUtc);
    }

    public static async Task<bool> SaveUserAggregateAsync(
        this IUserRepository userRepository,
        User user,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(user);

        user.EnsurePasswordIsSet();

        if (!user.IsChanged)
        {
            return true;
        }

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

        if (user.PasswordResetToken is null)
        {
            return await userRepository.ClearUserOneTimePasswordResetTokenAsync(user.Id, cancellationToken);
        }

        return await userRepository.SaveUserOneTimePasswordResetTokenAsync(
            user.Id,
            user.PasswordResetToken.Value,
            user.PasswordResetToken.ExpiresAt,
            cancellationToken);
    }
}
