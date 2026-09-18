using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Dto
{
    public sealed record AuthenticationResponseDto(
        string AccessToken,
        string RefreshToken,
        DateTimeOffset ExpiresAt
    );
}
