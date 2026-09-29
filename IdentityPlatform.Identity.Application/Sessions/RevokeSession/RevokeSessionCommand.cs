using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Sessions.RevokeSession
{
    public sealed record RevokeSessionCommand(Guid SessionId, Guid CurrentUserId)
      : ICommand<bool>;
}
