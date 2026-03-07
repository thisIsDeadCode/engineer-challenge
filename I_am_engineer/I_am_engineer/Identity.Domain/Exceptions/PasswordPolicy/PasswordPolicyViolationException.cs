using I_am_engineer.Identity.Domain.Exceptions;
namespace I_am_engineer.Identity.Domain.Exceptions.PasswordPolicy;

public sealed class PasswordPolicyViolationException : DomainException
{
    public PasswordPolicyViolationException(IReadOnlyCollection<string> violations)
        : base(BuildMessage(violations))
    {
        Violations = violations;
    }

    public IReadOnlyCollection<string> Violations { get; }

    private static string BuildMessage(IReadOnlyCollection<string> violations)
    {
        return violations.Count == 0
            ? "Password does not satisfy policy requirements."
            : string.Join(" ", violations);
    }
}
