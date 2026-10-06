using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Authorization
{
    public interface IAuthorizationReader
    {
        Task<UserAuthorizationData> GetUserAuthorizationDataAsync(Guid userId, CancellationToken cancellationToken);
    }
}
