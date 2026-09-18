using Common.Application.Abstractions;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.Logout
{
    public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool, IDomainError>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LogoutCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool, IDomainError>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            // 1. تشفير التوكن للبحث عنه
            var tokenHash = TokenSecurityHelper.HashToken(request.RefreshToken);

            // 2. جلب التوكن مع عائلته المرتبطة
            var storedToken = await _userRepository.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);

            // إذا لم يتم العثور على التوكن، نعتبر العملية ناجحة ظاهرياً (لتجنب تسريب معلومات للمهاجم)
            if (storedToken == null)
            {
                return Result.Success<bool, IDomainError>(true);
            }

            var family = storedToken.Family;

            // 3. التحقق مما إذا كانت العائلة ملغاة مسبقاً
            if (family.RevokedAt == null)
            {
                // 4. إلغاء عائلة التوكنات بالكامل لإنهاء الجلسة
                family.Revoke();

                // 5. حفظ التغييرات في قاعدة البيانات
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Success<bool, IDomainError>(true);
        }
    }
}
