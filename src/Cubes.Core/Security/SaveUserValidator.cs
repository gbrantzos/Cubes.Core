using FluentValidation;

namespace Cubes.Core.Security
{
    public class SaveUserValidator : AbstractValidator<SaveUser>
    {
        public SaveUserValidator()
        {
            RuleFor(x => x.UserDetails).NotNull().WithMessage("User details cannot be null!");
            RuleFor(x => x.UserDetails.UserName)
                .NotEmpty()
                .WithMessage("User name cannot be null!")
                .When(x => x.UserDetails != null);
            RuleFor(x => x.UserDetails.DisplayName)
                .NotEmpty()
                .WithMessage("Display name cannot be null!")
                .When(x => x.UserDetails != null);
            RuleFor(x => x.UserDetails.Roles)
                .NotEmpty()
                .WithMessage("Roles collection cannot be null!")
                .When(x => x.UserDetails != null);
        }
    }
}
