using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Sessions.RevokeAllUserSessions
{
    public sealed record RevokeAllUserSessionsCommand(Guid UserId)
    : ICommand<Result<bool, IDomainError>>;
}
