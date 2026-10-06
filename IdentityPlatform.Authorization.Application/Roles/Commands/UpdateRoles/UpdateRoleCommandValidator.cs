using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.UpdateRoles
{
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("RoleIDrequired");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("RoleNameRequired")
                .MaximumLength(100).WithMessage("RoleNameTooLong");

            RuleFor(x => x.Permissions)
                .NotEmpty().WithMessage("PermissionsRequired");
        }
    }
}
