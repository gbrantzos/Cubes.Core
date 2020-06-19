using Cubes.Core.Commands;
using FluentValidation;

namespace Cubes.Core.Security
{
    public class DeleteUser : Request<bool>
    {
        public string UserName { get; set; }
    }

    public class DeleteUserValidator : AbstractValidator<DeleteUser>
    {
        public DeleteUserValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("User name cannot be empty!");
        }
    }
}
