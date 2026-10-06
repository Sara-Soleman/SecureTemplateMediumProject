using Common.Application.Abstractions.CQRS;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.UpdateRoles
{
    public record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string Description,
    List<string> Permissions
) : ICommand<Result>;
}
