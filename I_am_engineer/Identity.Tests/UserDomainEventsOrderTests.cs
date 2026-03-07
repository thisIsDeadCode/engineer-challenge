using I_am_engineer.Identity.Application.Abstractions;
using I_am_engineer.Identity.Domain.Aggregates;
using I_am_engineer.Identity.Domain.DomainServices;
using I_am_engineer.Identity.Domain.ValueObjects;
using Moq;

namespace Identity.Tests;

public sealed class UserDomainEventsOrderTests
{
    [Fact]
    public void CreateNew_EmitsEventsInExpectedOrder()
    {
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(hasher => hasher.Hash(It.IsAny<string>()))
            .Returns(new PasswordHash("hashed-password"));

        var user = User.CreateNew("user@example.com", "StrongPassword1!", passwordHasher.Object, new PasswordPolicy());

        var eventNames = user.DomainEvents.Select(domainEvent => domainEvent.Name).ToArray();

        Assert.Equal(["UserRegistered", "PasswordChanged"], eventNames);
    }
}
