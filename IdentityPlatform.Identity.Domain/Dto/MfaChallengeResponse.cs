using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Dto
{
    public sealed record MfaChallengeResponse(
        Guid UserId,
        string MfaType, // "Totp" أو "Email"
        string Message
    );
}
