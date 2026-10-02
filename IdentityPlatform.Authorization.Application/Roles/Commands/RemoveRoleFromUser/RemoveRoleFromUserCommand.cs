using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.RemoveRoleFromUser
{
    public record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId) : ICommand<Unit>;
}
