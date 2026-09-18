using Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Sessions
{
    public sealed class UserSession : Entity<UserSession>
    {
        public Guid UserId { get; private set; }
        public string RefreshToken { get; private set; } = string.Empty;
        public string IpAddress { get; private set; } = string.Empty;
        public string UserAgent { get; private set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }

        // الخصائص المحسوبة (Calculated Properties) لضمان وضوح الحالة الأمنية
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Entity Framework Constructor (Protected/Private)
        private UserSession() { }

        private UserSession(
            Guid id,
            Guid userId,
            string refreshToken,
            string ipAddress,
            string userAgent,
            DateTimeOffset expiresAt) : base(id)
        {
            UserId = userId;
            RefreshToken = refreshToken;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            ExpiresAt = expiresAt;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        // Factory Method لإنشاء جلسة جديدة بأمان
        public static UserSession Create(
            Guid userId,
            string refreshToken,
            string ipAddress,
            string userAgent,
            DateTimeOffset expiresAt)
        {
            // يمكنك إضافة التحقق من صحة المدخلات هنا (Guard Clauses)
            return new UserSession(
                Guid.NewGuid(),
                userId,
                refreshToken,
                ipAddress,
                userAgent,
                expiresAt
            );
        }

        // سلوك إبطال الجلسة (Revocation) عند تسجيل الخروج أو الاشتباه الأمني
        public void Revoke()
        {
            if (!IsRevoked)
            {
                RevokedAt = DateTimeOffset.UtcNow;
            }
        }

        // تحديث الـ Refresh Token عند عملية الـ Token Refresh (Rotation)
        public void UpdateRefreshToken(string newRefreshToken, DateTimeOffset newExpiresAt)
        {
            RefreshToken = newRefreshToken;
            ExpiresAt = newExpiresAt;
        }
    }
}
