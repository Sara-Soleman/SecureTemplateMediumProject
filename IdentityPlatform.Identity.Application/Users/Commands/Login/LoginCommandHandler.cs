using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Application.Interfaces;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Domain.Dto;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Tokens.DTOs;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Enums;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.Login
{
    public sealed class LoginCommandHandler : CommandHandlerBase<LoginCommand, MfaChallengeResponse, IIdentityUnitOfWork>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService; // خدمة إرسال البريد الإلكتروني
        private User _user;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            IIdentityUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
            : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
           
        }



        protected async override Task<Result<MfaChallengeResponse, IDomainError>> ExecuteAsync(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. البحث عن المستخدم بالبريد أو اسم المستخدم
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail, cancellationToken);
            if (user == null)
            {
                // حماية أمنية: عدم توضيح ما إذا كان المستخدم موجوداً أم لا بالتحديد
                return Result.Failure<MfaChallengeResponse, IDomainError>(DomainError.InvalidCredentials());
            }
            _user = user;

            // 2. التحقق من كلمة المرور
            var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.Credential.PasswordHash);
            if (!isPasswordValid)
            {
                return Result.Failure<MfaChallengeResponse, IDomainError>(DomainError.InvalidCredentials());
            }

            // 3. بما أن الـ MFA إجباري للجميع، نقوم بالتوجيه حسب القناة المفضلة للمستخدم
            if (user.PreferredMfaType == MfaType.Email)
            {
                // توليد رمز عشوائي من 6 أرقام
                var randomCode = new Random().Next(100000, 999999).ToString();

                // حفظ الرمز مع صلاحية لمدة 5 دقائق مثلاً
                user.SetEmailOtp(randomCode, TimeSpan.FromMinutes(5));
               

                // إرسال الرمز عبر البريد الإلكتروني
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Security Verification Code",
                    $"Your verification code is: {randomCode}"
                );

                return Result.Success<MfaChallengeResponse, IDomainError>(
                    new MfaChallengeResponse(user.Id, "Email", "VerificationEmailSend")
                );
            }
            else
            {
                // إذا كان يستخدم الـ TOTP (Google Authenticator)
                return Result.Success<MfaChallengeResponse, IDomainError>(
                    new MfaChallengeResponse(user.Id, "Totp", "ProvideTOTP")
                );
            }
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<MfaChallengeResponse, IDomainError> result)
        {
            return _user;
        }
    }
}
