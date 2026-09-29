using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Sessions.RevokeAllUserSessions
{
    public sealed class RevokeAllUserSessionsCommandHandler
    : ICommandHandler<RevokeAllUserSessionsCommand, Result<bool, IDomainError>>
    {
        private readonly IUserSessionRepository _sessionRepository;
        

        public RevokeAllUserSessionsCommandHandler(
            IUserSessionRepository sessionRepository,
            IUnitOfWork unitOfWork)
        {
            _sessionRepository = sessionRepository;
            
        }

        public async Task<Result<Result<bool, IDomainError>, IDomainError>> Handle(
            RevokeAllUserSessionsCommand request,
            CancellationToken cancellationToken)
        {
            var activeSessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);

            foreach (var session in activeSessions)
            {
                session.Revoke();
                _sessionRepository.Update(session);
            }

            

            var innerSuccess = Result.Success<bool, IDomainError>(true);
            return Result.Success<Result<bool, IDomainError>, IDomainError>(innerSuccess);
        }
    }
}
