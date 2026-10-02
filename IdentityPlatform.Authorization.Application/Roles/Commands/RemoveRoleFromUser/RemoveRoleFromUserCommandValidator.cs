using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.RemoveRoleFromUser
{
    public class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
    {
        public RemoveRoleFromUserCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserIDrequired.");
            RuleFor(x => x.RoleId).NotEmpty().WithMessage("RoleIDrequired.");
        }
    }
}
