using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using IdentityPlatform.Identity.Domain.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Sessions.RevokeSession
{
    public sealed class RevokeSessionCommandHandler
        : CommandHandlerBase<RevokeSessionCommand, bool, IIdentityUnitOfWork>
    {
        private readonly IUserSessionRepository _sessionRepository;
        private UserSession _userSession;

        public RevokeSessionCommandHandler(
            IUserSessionRepository sessionRepository,
            IIdentityUnitOfWork unitOfWork, IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork)
        {
            _sessionRepository = sessionRepository;
        }

        

        protected async override Task<Result<bool, IDomainError>> ExecuteAsync(RevokeSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
            var userId = new Id<User>(request.CurrentUserId);
            if (session == null || session.UserId != userId)
            {
                return Result.Failure<bool, IDomainError>(DomainError.SessionNotFound());
                
            }

            session.Revoke();

            _sessionRepository.Update(session);
            
            return Result.Success<bool, IDomainError>(true);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<bool, IDomainError> result)
        {
            return _userSession;
        }
    }
}
