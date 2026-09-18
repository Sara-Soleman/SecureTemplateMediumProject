using Common.Application.Abstractions;
using Common.Application.Interfaces;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.ForgotPassword
{
    public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool, IDomainError>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public ForgotPasswordCommandHandler(
            IUserRepository userRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool, IDomainError>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
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

            // 4. حفظ التغييرات في قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. إرسال البريد الإلكتروني الذي يحتوي على رابط أو كود إعادة التعيين (rawToken)
            await _emailService.SendPasswordResetEmailAsync(user.Email, rawToken, cancellationToken);

            return Result.Success<bool, IDomainError>(true);
        }
    }
}
