using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Sessions.RevokeAllUserSessions
{
    public sealed class RevokeAllUserSessionsCommandHandler
    : CommandHandlerBase<RevokeAllUserSessionsCommand, bool>
    {
        private readonly IUserSessionRepository _sessionRepository;
        private UserSession _userSession;
        

        public RevokeAllUserSessionsCommandHandler(
            IUserSessionRepository sessionRepository,
            IUnitOfWork unitOfWork ,
            IDomainEventDispatcher domainEventDispatcher)
                : base(domainEventDispatcher, unitOfWork)

        {
            _sessionRepository = sessionRepository;
            
        }

  
        protected async override Task<Result<bool, IDomainError>> ExecuteAsync(RevokeAllUserSessionsCommand request, CancellationToken cancellationToken)
        {
            var activeSessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);

            foreach (var session in activeSessions)
            {
                session.Revoke();
                _sessionRepository.Update(session);
            }



            var innerSuccess = Result.Success<bool, IDomainError>(true);
            return Result.Success<bool, IDomainError>(true);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<bool, IDomainError> result)
        {
            return _userSession;
        }
    }
}
