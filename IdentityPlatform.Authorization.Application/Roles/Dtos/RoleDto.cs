using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Dtos
{
    public record RoleDto(Guid Id, string Name, string? Description, IEnumerable<string> Permissions);
}
