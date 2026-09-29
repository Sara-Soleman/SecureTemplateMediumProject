using Common.Application.Abstractions;
using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Application.Abstractions.Handlers;
using Common.Domain;
using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Dto;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Enums;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Application.Users.Commands.Login
{
    public sealed class VerifyLoginMfaCommandHandler : CommandHandlerBase<VerifyLoginMfaCommand, AuthenticationResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly IJwtTokenGenerator _tokenService; // خدمة إصدار الـ JWT و Refresh Token لديك
        private User _user;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VerifyLoginMfaCommandHandler(
            IUserRepository userRepository,
            ITotpService totpService,
            IJwtTokenGenerator tokenService,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IDomainEventDispatcher domainEventDispatcher)
: base(domainEventDispatcher, unitOfWork)
        {
            _userRepository = userRepository;
            _totpService = totpService;
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        

        protected async override Task<Result<AuthenticationResponseDto, IDomainError>> ExecuteAsync(VerifyLoginMfaCommand request, CancellationToken cancellationToken)
        {
            // 1. جلب المستخدم
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<AuthenticationResponseDto, IDomainError>(DomainError.UserNotFound());
            }
            _user = user;
            bool isCodeValid = false;

            // 2. التحقق بناءً على القناة المستخدمة
            if (user.PreferredMfaType == MfaType.Email)
            {
                isCodeValid = user.VerifyEmailOtp(request.Code);
            }
            else
            {
                if (string.IsNullOrEmpty(user.MfaSecret))
                {
                    return Result.Failure<AuthenticationResponseDto, IDomainError>(DomainError.InvalidMfaCode());
                }
                isCodeValid = _totpService.VerifyCode(user.MfaSecret, request.Code);
            }

            if (!isCodeValid)
            {
                return Result.Failure<AuthenticationResponseDto, IDomainError>(DomainError.InvalidMfaCode());
            }
            var httpContext = _httpContextAccessor.HttpContext;

            // استخراج الـ IP الحقيقي مع مراعاة الـ Proxies
            var ipAddress = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? httpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString()
                            ?? "Unknown";

            // استخراج الـ User-Agent (متصفح المستخدم أو الجهاز)
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "Unknown";
            }
            // 3. نجاح التحقق: تنظيف أي رموز مؤقتة وإصدار التوكنات النهائية
            // (يمكنك إضافة منطق توليد عائلة التوكنات هنا حسب نظامك الحالي)
            var authResponse = await _tokenService.GenerateTokensAsync(user, ipAddress, userAgent, cancellationToken: cancellationToken);

            return Result.Success<AuthenticationResponseDto, IDomainError>(authResponse);
        }

        protected override IAggregateRoot? GetAggregateRoot(Result<AuthenticationResponseDto, IDomainError> result)
        {
            return _user;
        }
    }
}
