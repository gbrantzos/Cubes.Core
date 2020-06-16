using System;
using Cubes.Core.Commands;
using FluentValidation;

namespace Cubes.Core.Security
{
    public class ResetUserPassword : Request<bool>
    {
        public string UserName { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetUserPasswordValidator : AbstractValidator<ResetUserPassword>
    {
        public ResetUserPasswordValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("User name cannot be empty!");
            RuleFor(x => x.OldPassword).NotEmpty().WithMessage("Old password cannot be empty!");
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password cannot be empty!");
            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.OldPassword)
                .When(x => !String.IsNullOrEmpty(x.OldPassword))
                .WithMessage("New password cannot be the same with existing!");
        }
    }
}
