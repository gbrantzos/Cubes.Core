using FluentValidation;

namespace Cubes.Core.Commands.Basic
{
    public class RunOsProcessValidator : AbstractValidator<RunOsProcess>
    {
        public RunOsProcessValidator()
        {
            RuleFor(x => x.Command).NotEmpty().WithMessage("Command cannot be empty!");
        }
    }
}
