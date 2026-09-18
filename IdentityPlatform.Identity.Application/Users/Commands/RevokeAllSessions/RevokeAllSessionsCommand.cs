using Common.Application.Abstractions.CQRS;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RevokeAllSessions
{
    public sealed record RevokeAllSessionsCommand(
    Guid UserId
) : ICommand<bool>;
}
