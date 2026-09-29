using Common.Application.Abstractions;
using IdentityPlatform.Identity.Domain.Dto;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Tokens.Interfaces;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly IUserSessionRepository _sessionRepository;
        //private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public JwtTokenGenerator(
            IConfiguration configuration,
            IUserSessionRepository sessionRepository,
            IRefreshTokenRepository refreshTokenRepository
            //IUnitOfWork unitOfWork
            )
        {
            _configuration = configuration;
            _sessionRepository = sessionRepository;
            //_unitOfWork = unitOfWork;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public JwtTokenGenerator()
        {
        }

        // هذه هي دالتك الأصلية لتوليد الـ Access Token
        public string GenerateToken(User user, Guid sessionId)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("sid", sessionId.ToString()), // ربط التوكن بالجلسة الفريدة
            new Claim("tokenVersion", user.TokenVersion.ToString())
        };

            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "YourSuperSecretKeyHereThatIsLongEnough12345!"));
            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var token = new JwtSecurityToken(
            //    issuer: _configuration["Jwt:Issuer"],
            //    audience: _configuration["Jwt:Audience"],
            //    claims: claims,
            //    expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60")),
            //    signingCredentials: creds
            //);
            var issuer = _configuration["Jwt:Issuer"] ?? "IdentityPlatform";
            var audience = _configuration["Jwt:Audience"] ?? "IdentityPlatformApi";
            var secret = _configuration["Jwt:Secret"] ?? "YourSuperSecretKeyHereThatIsLongEnough12345!";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // دالة توليد الـ Refresh Token العشوائي الآمن
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // الدالة المعدلة لتخزين الجلسة حقيقياً في قاعدة البيانات
        public async Task<AuthenticationResponseDto> GenerateTokensAsync(
            User user,
            string ipAddress,
            string userAgent,
            CancellationToken cancellationToken)
        {
            // 1. توليد Refresh Token عشوائي
            var rawRefreshToken = GenerateRefreshToken();

            // ب. تشفير الـ Refresh Token للحصول على الـ Hash (للتخزين الآمن في جدول RefreshTokens)
            var tokenHash = TokenSecurityHelper.HashToken(rawRefreshToken);

            // 2. تحديد مدة صلاحية الـ Refresh Token (مثلاً 7 أيام أو حسب الإعدادات)
            var refreshTokenExpiryDays = double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
            var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshTokenExpiryDays);

            var lifetime = TimeSpan.FromDays(refreshTokenExpiryDays);

            // 1. جلب كل الجلسات النشطة الحالية للمستخدم
            var activeSessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(user.Id.Value, cancellationToken);

            foreach (var oldSession in activeSessions)
            {
                oldSession.Revoke(); // أو حذفها نهائياً .Remove() حسب تفضيلك، ولكن الـ Revoke أفضل للحفاظ على الـ Audit
                _sessionRepository.Update(oldSession);
            }

          

            // 3. إنشاء معرف جلسة جديد وكيان الجلسة
            var sessionId = Guid.NewGuid();
            var session = UserSession.Create(
                user.Id.Value, // تأكد إذا كان الـ UserId يتطلب .Value أو يُمرر مباشرة
                tokenHash,
                ipAddress,
                userAgent,
                refreshTokenExpiresAt
            );
            var family = RefreshTokenFamily.Create(
                userId: user.Id,
                sessionId: sessionId,
                ipAddress: ipAddress,
                userAgent: userAgent,
                lifetime: lifetime
            );


            await _refreshTokenRepository.AddFamilyAsync(family, cancellationToken);

            var refreshTokenEntity = RefreshToken.Create(
                familyId: family.Id,
                tokenHash: tokenHash,
                lifetime: lifetime,
                ipAddress,
                userAgent
            );
            await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

            // 4. حفظ الجلسة في قاعدة البيانات عبر المستودع
            await _sessionRepository.AddAsync(session, cancellationToken);
           // await _unitOfWork.SaveChangesAsync(cancellationToken);


            // 5. توليد الـ Access Token المرتبط بمعرّف الجلسة (sessionId) الفعلي
            var accessToken = GenerateToken(user, session.Id);

            var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");
            var accessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

            return new AuthenticationResponseDto(
                AccessToken: accessToken,
                RefreshToken: rawRefreshToken,
                ExpiresAt: accessTokenExpiresAt
            );
        }
        //public string GenerateToken(User user, Guid sessionId)
        //{
        //    var claims = new List<Claim>
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.Id.Value.ToString()),
        //        new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
        //        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        //        new Claim("sessionId", sessionId.ToString()),
        //        new Claim("tokenVersion", user.TokenVersion.ToString())
        //    };

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "SuperSecretKeyForDevelopmentOnly12345!"));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60")),
        //        signingCredentials: creds
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }
}
