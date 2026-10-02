using Common.Application.Abstractions;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Tokens.DTOs;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler : CommandHandlerBase<RefreshTokenCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRoleRepository _roleRepository;
        private User _user;

        public RefreshTokenCommandHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IDomainEventDispatcher domainEventDispatcher)
                : base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _roleRepository = roleRepository;
        }

        

        protected async override Task<Result<AuthResponseDto, IDomainError>> ExecuteAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
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

            // 🔒 3.1. [التحقق الأمني الأحدث]: مطابقة عنوان الـ IP أو بصمة الجهاز لمنع سرقة واستخدام التوكن من شبكة/جهاز آخر
            if (!string.Equals(family.IpAddress, request.IpAddress, StringComparison.OrdinalIgnoreCase))
            {
                // إذا اختلف الـ IP، فهذه محاولة اختراق محتملة (تمت سرقة الـ Token)، نقوم بإلغاء العائلة فوراً!
                family.Revoke();
               
                return Result.Failure<AuthResponseDto, IDomainError>(DomainError.SecurityAlertTokenReuseDetected());
            }

            // 4. كشف محاولة الاستخدام المتكرر (Token Reuse Detection):
            if (storedToken.ConsumedAt != null)
            {
                family.Revoke();
               
                return Result.Failure<AuthResponseDto, IDomainError>(DomainError.SecurityAlertTokenReuseDetected());
            }

            // 5. جلب المستخدم المرتبط بالجلسة أو العائلة للتأكد من حالته
            var user = await _userRepository.GetBySessionIdAsync(family.SessionId, cancellationToken);
            if (user == null || user.AccountStatus != AccountStatus.Active)
            {
                return Result.Failure<AuthResponseDto, IDomainError>(DomainError.AccountIsInactive());
            }

            // 6. إنشاء توكن جديد ضمن نفس العائلة (Token Rotation)
            var newRawToken = TokenSecurityHelper.GenerateSecureTokenString();
            var newTokenHash = TokenSecurityHelper.HashToken(newRawToken);

            // استدعاء دالة الإنشاء الصحيحة مع تمرير المعرف الجديد للتوكن وتثبيت العائلة
            var newRefreshToken = IdentityPlatform.Identity.Domain.Tokens.RefreshToken.Create(
                familyId: family.Id,
                tokenHash: newTokenHash,
                lifetime: TimeSpan.FromDays(7),
                ipAddress: request.IpAddress,   // تمرير الـ IP
                userAgent: request.UserAgent,   // تمرير الـ UserAgent
                id: Id<IdentityPlatform.Identity.Domain.Tokens.RefreshToken>.New()
            );

            family.AddRefreshToken(newRefreshToken);

            // 7. استهلاك التوكن القديم وربطه بالتوكن الجديد
            storedToken.Consume(newRefreshToken.Id);

            var userRoles = await _roleRepository.GetRolesByUserIdAsync(user.Id.Value, cancellationToken);

            var roleNames = userRoles.Select(r => r.Name).ToList();
            var permissions = userRoles
                .SelectMany(r => r.Permissions)
                .Distinct()
                .ToList();

            // 8. توليد JWT Access Token جديد
            var newAccessToken = _jwtTokenGenerator.GenerateToken(user, family.SessionId, roleNames, permissions);

            

            // 10. إرجاع التوكنات الجديدة بنجاح
            return Result.Success<AuthResponseDto, IDomainError>(new AuthResponseDto(newAccessToken, newRawToken));
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<AuthResponseDto, IDomainError> result)
        {
            return _user;
        }
    }
}
