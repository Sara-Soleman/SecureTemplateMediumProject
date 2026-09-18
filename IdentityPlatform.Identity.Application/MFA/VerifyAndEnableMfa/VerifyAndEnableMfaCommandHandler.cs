using Common.Application.Abstractions;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.MFA.VerifyAndEnableMfa
{
    public sealed class VerifyAndEnableMfaCommandHandler : IRequestHandler<VerifyAndEnableMfaCommand, Result<bool, IDomainError>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyAndEnableMfaCommandHandler(IUserRepository userRepository, ITotpService totpService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _totpService = totpService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool, IDomainError>> Handle(VerifyAndEnableMfaCommand request, CancellationToken cancellationToken)
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success<bool, IDomainError>(true);
        }
    }
}
