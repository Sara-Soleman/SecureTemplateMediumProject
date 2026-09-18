using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Sessions.RevokeSession
{
    public sealed class RevokeSessionCommandHandler
        : IRequestHandler<RevokeSessionCommand, Result<Result<bool, IDomainError>, IDomainError>>
    {
        private readonly IUserSessionRepository _sessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RevokeSessionCommandHandler(
            IUserSessionRepository sessionRepository,
            IUnitOfWork unitOfWork)
        {
            _sessionRepository = sessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Result<bool, IDomainError>, IDomainError>> Handle(
            RevokeSessionCommand request,
            CancellationToken cancellationToken)
        {
            var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

            if (session == null || session.UserId != request.CurrentUserId)
            {
                Result<bool, IDomainError> innerFailure = Result.Failure<bool, IDomainError>(DomainError.SessionNotFound());
                return Result.Success<Result<bool, IDomainError>, IDomainError>(innerFailure);
            }

            session.Revoke();

            _sessionRepository.Update(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            Result<bool, IDomainError> innerSuccess = Result.Success<bool, IDomainError>(true);
            return Result.Success<Result<bool, IDomainError>, IDomainError>(innerSuccess);
        }
    }
}
