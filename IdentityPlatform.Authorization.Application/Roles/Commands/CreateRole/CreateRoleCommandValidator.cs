using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.CreateRole
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("RoleNameRequired")
                .MaximumLength(100).WithMessage("RoleNameLong");

            RuleFor(x => x.Description)
                .MaximumLength(250).WithMessage("DescriptionLong");

            RuleFor(x => x.Permissions)
                .NotNull().WithMessage("PermissionsNotNull");
        }
    }
}
