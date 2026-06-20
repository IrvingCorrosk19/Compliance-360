using Compliance360.Shared;
using Microsoft.Extensions.Options;

namespace Compliance360.Application.Identity;

public sealed class PasswordPolicyValidator : IPasswordPolicyValidator
{
    private readonly PasswordPolicyOptions _options;

    public PasswordPolicyValidator(IOptions<PasswordPolicyOptions> options)
    {
        _options = options.Value;
    }

    public Result Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Failure("Password is required.");
        }

        if (password.Length < _options.MinimumLength)
        {
            return Result.Failure($"Password must be at least {_options.MinimumLength} characters.");
        }

        if (_options.RequireUppercase && !password.Any(char.IsUpper))
        {
            return Result.Failure("Password must include an uppercase letter.");
        }

        if (_options.RequireLowercase && !password.Any(char.IsLower))
        {
            return Result.Failure("Password must include a lowercase letter.");
        }

        if (_options.RequireDigit && !password.Any(char.IsDigit))
        {
            return Result.Failure("Password must include a digit.");
        }

        if (_options.RequireSymbol && !password.Any(character => !char.IsLetterOrDigit(character)))
        {
            return Result.Failure("Password must include a symbol.");
        }

        return Result.Success();
    }
}
