using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Tokens.DTOs
{
    public sealed record AuthResponseDto(string AccessToken, string RefreshToken);
}
