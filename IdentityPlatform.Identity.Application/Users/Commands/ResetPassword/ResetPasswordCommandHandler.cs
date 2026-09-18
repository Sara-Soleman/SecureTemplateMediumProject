using Common.Application.Abstractions;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.ResetPassword
{
    public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool, IDomainError>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool, IDomainError>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // 1. جلب المستخدم مع الـ Credential المرتبطة به
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.Email, cancellationToken);
            if (user == null || user.Credential == null)
            {
                return Result.Failure<bool, IDomainError>(DomainError.InvalidOrExpiredPasswordResetToken());
            }

            // 2. تشفير كلمة المرور الجديدة وتشفير التوكن للمقارنة
            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            var tokenHash = TokenSecurityHelper.HashToken(request.Token);

            try
            {
                // 3. استدعاء دالة الـ Domain الموجودة في كلاس User 
                // (والتي تتحقق داخلياً من تطابق الـ TokenHash وتاريخ الصلاحية ExpiresAt)
                user.ResetPassword(newPasswordHash, tokenHash);

                // 4. إبطال كافة الجلسات السابقة عبر رفع إصدار التوكن
                user.IncrementTokenVersion();

                // 5. حفظ التغييرات بقاعدة البيانات
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success<bool, IDomainError>(true);
            }
            catch (Exception)
            {
                // إذا فشل التحقق في الـ Domain (مثل انتهاء الصلاحية أو خطأ التوكن) سترمي استثناءً نلتقطه هنا لنعيد خطأً نظيفاً
                return Result.Failure<bool, IDomainError>(DomainError.InvalidOrExpiredPasswordResetToken());
            }
        }
    }
}
