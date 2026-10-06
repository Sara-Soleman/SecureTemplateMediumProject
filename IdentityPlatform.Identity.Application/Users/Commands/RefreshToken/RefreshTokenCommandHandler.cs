using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Application.Helpers;
using IdentityPlatform.Identity.Application.Persistence;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Tokens.DTOs;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler : CommandHandlerBase<RefreshTokenCommand, AuthResponseDto, IIdentityUnitOfWork>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private User? _user; // 👈 جعلناه قابلاً للقيم الفارغة لتجنب التحذيرات

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IIdentityUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
                : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        protected override async Task<Result<AuthResponseDto, IDomainError>> ExecuteAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            // 1. تشفير التوكن القادم للبحث عنه في قاعدة البيانات
            var tokenHash = TokenSecurityHelper.HashToken(request.RefreshToken);

            // 2. البحث عن الـ Refresh Token في قاعدة البيانات مع عائلته
            var storedToken = await _userRepository.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);

            if (storedToken == null || storedToken.ExpiresAt <= DateTimeOffset.UtcNow || storedToken.RevokedAt != null)
            {
                return Result.Failure<AuthResponseDto, IDomainError>(DomainError.InvalidRefreshToken());
            }

            var family = storedToken.Family;

            // 3. التحقق مما إذا كانت عائلة التوكنات ملغاة مسبقاً
            if (family.RevokedAt != null)
            {
                return Result.Failure<AuthResponseDto, IDomainError>(DomainError.InvalidRefreshToken());
            }

            // 🔒 3.1. [التحقق الأمني]: مطابقة عنوان الـ IP لمنع سرقة واستخدام التوكن من شبكة أخرى
            if (!string.Equals(family.IpAddress, request.IpAddress, StringComparison.OrdinalIgnoreCase))
            {
                family.Revoke();
                return Result.Failure<AuthResponseDto, IDomainError>(DomainError.SecurityAlertTokenReuseDetected());
            }

            // 4. كشف محاولة الاستخدام المتكرر (Token Reuse Detection)
            if (storedToken.ConsumedAt != null)
            {
                family.Revoke();
                return Result.Failure<AuthResponseDto, IDomainError>(DomainError.SecurityAlertTokenReuseDetected());
            }

            // 5. جلب المستخدم المرتبط بالجلسة أو العائلة للتأكد من حالته
            _user = await _userRepository.GetBySessionIdAsync(family.SessionId, cancellationToken);
            if (_user == null || _user.AccountStatus != AccountStatus.Active)
            {
                return Result.Failure<AuthResponseDto, IDomainError>(DomainError.AccountIsInactive());
            }

            // 6. إنشاء توكن جديد ضمن نفس العائلة (Token Rotation)
            var newRawToken = TokenSecurityHelper.GenerateSecureTokenString();
            var newTokenHash = TokenSecurityHelper.HashToken(newRawToken);

            var newRefreshToken = IdentityPlatform.Identity.Domain.Tokens.RefreshToken.Create(
                familyId: family.Id,
                tokenHash: newTokenHash,
                lifetime: TimeSpan.FromDays(7),
                ipAddress: request.IpAddress,   
                userAgent: request.UserAgent    
            );

            family.AddRefreshToken(newRefreshToken);

            // 7. استهلاك التوكن القديم وربطه بالتوكن الجديد
            storedToken.Consume(newRefreshToken.Id);

            // 8. توليد JWT Access Token جديد
        
            var newAccessToken = await _jwtTokenGenerator.GenerateTokensAsync(_user, request.IpAddress, request.UserAgent, cancellationToken);
            var responseDto = new AuthResponseDto(newAccessToken.AccessToken, newRawToken);

            return Result.Success<AuthResponseDto, IDomainError>(responseDto);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<AuthResponseDto, IDomainError> result)
        {
            return _user; 
        }
    }
}
