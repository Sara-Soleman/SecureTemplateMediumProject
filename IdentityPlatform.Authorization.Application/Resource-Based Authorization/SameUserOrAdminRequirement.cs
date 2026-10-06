using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Resource_Based_Authorization
{
    public class SameUserOrAdminRequirement : IAuthorizationRequirement
    {
        // يمكنك إضافة شروط إضافية هنا إن أردت
    }
}
