using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Sessions
{
    public sealed record GetActiveSessionsQuery(Guid UserId) : IQuery<Result<IEnumerable<UserSessionDto>, IDomainError>>;


   
}
