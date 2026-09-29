using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Application.Interfaces;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.ForgotPassword
{
    public sealed class ForgotPasswordCommandHandler : CommandHandlerBase<ForgotPasswordCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private User _user;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
: base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }


        protected async override Task<Result<bool, IDomainError>> ExecuteAsync(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // 1. البحث عن المستخدم باستخدام البريد الإلكتروني
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.Email, cancellationToken);

            // للأمان والحماية من هجمات تخمين الإيميلات (User Enumeration)، 
            // إذا لم يتم العثور على المستخدم نعيد نجاحاً وهمياً (Success) دون إفشاء أي معلومة.
            if (user == null)
            {
                return Result.Success<bool, IDomainError>(true);
            }

            // 2. توليد توكن استعادة كلمة المرور عشوائي ومشفر
            var rawToken = TokenSecurityHelper.GenerateSecureTokenString();
            var tokenHash = TokenSecurityHelper.HashToken(rawToken);
            var expiresAt = DateTimeOffset.UtcNow.AddHours(1); // صلاحية لمدة ساعة واحدة

            // 3. استدعاء دالة الـ Domain الموجودة في كلاس الـ User لتحديث التوكن وإطلاق الحدث (Domain Event)
            user.SetPasswordResetToken(tokenHash, expiresAt);

            // 5. إرسال البريد الإلكتروني الذي يحتوي على رابط أو كود إعادة التعيين (rawToken)
            await _emailService.SendPasswordResetEmailAsync(user.Email, rawToken, cancellationToken);

            return Result.Success<bool, IDomainError>(true);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<bool, IDomainError> result)
        {
            return _user;
        }
    }
}
