using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Dtos
{
    public record UserRolesAndPermissionsDto(
        IEnumerable<string> Roles,
        IEnumerable<string> Permissions
    );
}
