using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Dtos
{
    public record UpdateRoleRequestDto(
    string Name,
    string Description,
    List<string> Permissions
);
}
