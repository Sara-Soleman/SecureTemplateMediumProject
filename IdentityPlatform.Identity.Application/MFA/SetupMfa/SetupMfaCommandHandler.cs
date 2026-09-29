using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Dto;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.MFA.SetupMfa
{
    public sealed class SetupMfaCommandHandler : CommandHandlerBase<SetupMfaCommand, MfaSetupResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private User? _user; // حقل خاص لتخزين الكيان وإرجاعه لنشر الأحداث

        public SetupMfaCommandHandler(
            IUserRepository userRepository,
            ITotpService totpService,
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _totpService = totpService;
        }


        protected override async Task<Result<MfaSetupResponseDto, IDomainError>> ExecuteAsync(
            SetupMfaCommand request,
            CancellationToken cancellationToken)
        {
            _user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (_user == null)
            {
                return Result.Failure<MfaSetupResponseDto, IDomainError>(DomainError.UserNotFound());
            }

            // 1. توليد سر جديد ورابط الـ QR
            var secret = _totpService.GenerateSecretKey();
            var qrCodeUri = _totpService.GetQrCodeUri(_user.Email, secret);

            // 2. تحديث الكيان داخلياً (تطبيق مبدأ الـ Encapsulation وحفظ الحدث ضمن الـ Domain)
             _user.SetupMfaSecret(secret);
            
            var responseDto = new MfaSetupResponseDto(secret, qrCodeUri);
            return Result.Success<MfaSetupResponseDto, IDomainError>(responseDto);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<MfaSetupResponseDto, IDomainError> result)
        {
            // إرجاع الكيان لكي يقوم الـ Dispatcher بنشر أي Domain Events ناتجة عن عملية الـ Setup
            return _user;
        }
    }
}
