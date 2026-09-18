using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Dto
{
    public sealed record UserSessionDto(
       Guid SessionId,
       string IpAddress,
       string UserAgent,
       DateTimeOffset CreatedAt,
       bool IsCurrent
   );
}
