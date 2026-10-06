using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Authorization
{
    public sealed record UserAuthorizationData(
        IEnumerable<string> Roles,
        IEnumerable<string> Permissions
    );
}
