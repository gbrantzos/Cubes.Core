using System;
using System.Linq;
using FluentValidation;

namespace Cubes.Core.Security
{
    public class SaveRolesValidator : AbstractValidator<SaveRoles>
    {
        public SaveRolesValidator()
        {
            RuleForEach(x => x.Roles)
                .ChildRules(r =>
                {
                    r.RuleFor(i => i.Code).NotEmpty().WithMessage("Rule code cannot be null");
                    r.RuleFor(i => i.Description).NotEmpty().WithMessage("Rule description cannot be null");
                    r.RuleFor(i => i.Code)
                        .NotEqual("cubes", StringComparer.OrdinalIgnoreCase)
                        .WithMessage("Cannot use 'cubes' as user name!");
                });
            RuleFor(x => x.Roles)
                .Must(x => x.Select(r => r.Code).Distinct().Count() == x.Count())
                .WithMessage("Duplicate roles found!");
        }
    }
}

