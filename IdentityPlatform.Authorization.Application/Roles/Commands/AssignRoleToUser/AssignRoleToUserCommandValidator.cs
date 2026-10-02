using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.AssignRoleToUser
{
    public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
    {
        public AssignRoleToUserCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserIDrequired.");
            RuleFor(x => x.RoleId).NotEmpty().WithMessage("RoleIDrequired.");
        }
    }
}
