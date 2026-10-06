using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RevokeAllSessions
{
    public sealed class RevokeAllSessionsCommandHandler : CommandHandlerBase<RevokeAllSessionsCommand, bool, IIdentityUnitOfWork>
    {
        private readonly IUserRepository _userRepository;
        private User _user;

        public RevokeAllSessionsCommandHandler(
            IUserRepository userRepository,
            IIdentityUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
           
        }



        protected async override Task<Result<bool, IDomainError>> ExecuteAsync(RevokeAllSessionsCommand request, CancellationToken cancellationToken)
        {
            // 1. جلب المستخدم
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<bool, IDomainError>(DomainError.UserNotFound());
            }

            // 2. استدعاء ميثود زيادة الإصدار الموجودة في الكيان لإبطال كافة التوكنات السابقة
            user.IncrementTokenVersion();

            return Result.Success<bool, IDomainError>(true);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<bool, IDomainError> result)
        {
            return _user;
        }
    }
}
