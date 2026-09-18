using Common.Application.Abstractions;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Dto;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.MFA.SetupMfa
{
    public sealed class SetupMfaCommandHandler : IRequestHandler<SetupMfaCommand, Result<MfaSetupResponseDto, IDomainError>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly IUnitOfWork _unitOfWork;

        public SetupMfaCommandHandler(IUserRepository userRepository, ITotpService totpService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _totpService = totpService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<MfaSetupResponseDto, IDomainError>> Handle(SetupMfaCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result.Failure<MfaSetupResponseDto, IDomainError>(DomainError.UserNotFound());

            // 1. توليد سر جديد
            var secret = _totpService.GenerateSecretKey();
            var qrCodeUri = _totpService.GetQrCodeUri(user.Email, secret);

            // 2. حفظ السر مؤقتاً في الكيان (لم يفعّل نهائياً حتى يتم تأكيد الرمز)
            user.SetupMfaSecret(secret);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success<MfaSetupResponseDto, IDomainError>(new MfaSetupResponseDto(secret, qrCodeUri));
        }
    }
}
