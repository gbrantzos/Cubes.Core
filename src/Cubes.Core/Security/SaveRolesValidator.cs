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
                });
        }
    }
}
