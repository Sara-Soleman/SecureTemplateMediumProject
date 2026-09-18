using Common.Application.Abstractions.CQRS;
using IdentityPlatform.Identity.Domain.Users.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.MFA.ChangeMfaType
{
    public sealed record ChangeMfaPreferenceCommand(
        Guid UserId,
        MfaType NewMfaType // Email أو Totp
    ) : ICommand<bool>;
}
