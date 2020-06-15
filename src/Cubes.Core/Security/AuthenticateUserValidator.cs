using FluentValidation;

namespace Cubes.Core.Security
{
    public class AuthenticateUserValidator : AbstractValidator<AuthenticateUser>
    {
        public AuthenticateUserValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName cannot be empty!");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password cannot be empty!");
        }
    }
}
