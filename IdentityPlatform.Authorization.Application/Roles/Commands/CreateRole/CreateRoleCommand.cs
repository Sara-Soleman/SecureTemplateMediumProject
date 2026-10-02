using Common.Application.Abstractions.CQRS;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Commands.CreateRole
{
    public record CreateRoleCommand(
        string Name,
        string? Description,
        List<string> Permissions
    ) : ICommand<Guid>;
}
