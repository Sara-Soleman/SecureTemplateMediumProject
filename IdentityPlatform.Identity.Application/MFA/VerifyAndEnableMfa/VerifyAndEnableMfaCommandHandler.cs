using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Application.Events.Dispatchers;
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

namespace IdentityPlatform.Identity.Application.MFA.VerifyAndEnableMfa
{
    public sealed class VerifyAndEnableMfaCommandHandler : CommandHandlerBase<VerifyAndEnableMfaCommand, bool, IIdentityUnitOfWork>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private User _user;

        public VerifyAndEnableMfaCommandHandler(IUserRepository userRepository, ITotpService totpService, IIdentityUnitOfWork unitOfWork, IDomainEventDispatcher domainEventDispatcher)
             : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _totpService = totpService;
        }



        protected async override Task<Result<bool, IDomainError>> ExecuteAsync(VerifyAndEnableMfaCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null || string.IsNullOrEmpty(user.MfaSecret))
                return Result.Failure<bool, IDomainError>(DomainError.UserNotFound());

            // 1. التحقق من صحة الرمز المدخل عبر خدمة الـ TOTP
            var isValid = _totpService.VerifyCode(user.MfaSecret, request.Code);
            if (!isValid)
            {
                return Result.Failure<bool, IDomainError>(DomainError.InvalidMfaCode());
            }

            // 2. تفعيل الميزة رسمياً
            user.VerifyAndEnableMfa(user.MfaSecret);

            return Result.Success<bool, IDomainError>(true);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<bool, IDomainError> result)
        {
            return _user;
        }
    }
}
