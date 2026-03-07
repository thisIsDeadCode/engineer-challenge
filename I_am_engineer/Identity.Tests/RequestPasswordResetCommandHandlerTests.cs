using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Application.Commands;
using I_am_engineer.Identity.Application.Handlers.Commands;
using I_am_engineer.Identity.Domain.Aggregates;
using I_am_engineer.Identity.Domain.ValueObjects;
using Moq;

namespace Identity.Tests;

public sealed class RequestPasswordResetCommandHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsSuccess_WhenUserDoesNotExist()
    {
        var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        var tokenGenerator = new Mock<IPasswordResetTokenGenerator>(MockBehavior.Strict);

        userRepository
            .Setup(repository => repository.GetByEmailAsync("missing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new RequestPasswordResetCommandHandler(userRepository.Object, tokenGenerator.Object);

        var response = await handler.Handle(new RequestPasswordResetCommand("missing@example.com"), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Null(response.Message);

        userRepository.Verify(repository => repository.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenResetRequestedTooFrequently()
    {
        var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        var tokenGenerator = new Mock<IPasswordResetTokenGenerator>(MockBehavior.Strict);
        var now = DateTimeOffset.UtcNow;
        var user = User.Restore(
            id: Guid.NewGuid(),
            email: "test@example.com",
            passwordHash: "hashed-password",
            passwordResetTokenValue: "existing-reset-token",
            passwordResetTokenIsUsed: false,
            passwordResetTokenExpiresAt: now.AddMinutes(10),
            passwordResetTokenIssuedAt: now,
            failedLoginAttempts: 0,
            lockedUntilUtc: null,
            isActive: true,
            session: null,
            createdAtUtc: now.AddDays(-1),
            updatedAtUtc: now.AddDays(-1));

        userRepository
            .Setup(repository => repository.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new RequestPasswordResetCommandHandler(userRepository.Object, tokenGenerator.Object);

        var response = await handler.Handle(new RequestPasswordResetCommand("test@example.com"), CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal("Password reset request is too frequent. Please try again later.", response.Message);

        userRepository.Verify(repository => repository.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_IssuesTokenAndPersistsUser_WhenRequestAllowed()
    {
        var userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        var tokenGenerator = new Mock<IPasswordResetTokenGenerator>(MockBehavior.Strict);
        var now = DateTimeOffset.UtcNow;
        var user = User.Restore(
            id: Guid.NewGuid(),
            email: "test@example.com",
            passwordHash: "hashed-password",
            passwordResetTokenValue: "old-reset-token",
            passwordResetTokenIsUsed: true,
            passwordResetTokenExpiresAt: now.AddMinutes(-1),
            passwordResetTokenIssuedAt: now.AddHours(-2),
            failedLoginAttempts: 0,
            lockedUntilUtc: null,
            isActive: true,
            session: null,
            createdAtUtc: now.AddDays(-1),
            updatedAtUtc: now.AddDays(-1));
        var generatedToken = PasswordResetToken.Create("new-reset-token", false, now.AddMinutes(30));

        userRepository
            .Setup(repository => repository.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        tokenGenerator
            .Setup(generator => generator.GenerateToken())
            .Returns(generatedToken);
        userRepository
            .Setup(repository => repository.SaveAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RequestPasswordResetCommandHandler(userRepository.Object, tokenGenerator.Object);

        var response = await handler.Handle(new RequestPasswordResetCommand("test@example.com"), CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Null(response.Message);
        Assert.NotNull(user.PasswordResetToken);
        Assert.Equal("new-reset-token", user.PasswordResetToken!.Value);

        userRepository.Verify(repository => repository.SaveAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
