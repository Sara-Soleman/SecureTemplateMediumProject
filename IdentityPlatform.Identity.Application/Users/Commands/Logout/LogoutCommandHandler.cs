using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Application.Helpers;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.Logout
{
    public sealed class LogoutCommandHandler : CommandHandlerBase<LogoutCommand, bool, IIdentityUnitOfWork>
    {
        private readonly IUserRepository _userRepository;
        private User _user;

        public LogoutCommandHandler(
            IUserRepository userRepository,
            IIdentityUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
                : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            
        }

        

        protected async override Task<Result<bool, IDomainError>> ExecuteAsync(LogoutCommand request, CancellationToken cancellationToken)
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
            _user = await _userRepository.GetBySessionIdAsync(family.SessionId, cancellationToken);
            // 3. التحقق مما إذا كانت العائلة ملغاة مسبقاً
            if (family.RevokedAt == null)
            {
                // 4. إلغاء عائلة التوكنات بالكامل لإنهاء الجلسة
                family.Revoke();

                
            }

            return Result.Success<bool, IDomainError>(true);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<bool, IDomainError> result)
        {
            return _user;
        }
    }
}
