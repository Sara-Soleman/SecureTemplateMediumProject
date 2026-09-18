using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Dto
{
    public sealed record MfaSetupResponseDto(string Secret, string QrCodeUri);
}
