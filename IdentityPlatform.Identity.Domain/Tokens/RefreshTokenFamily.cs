using Common.Domain;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Collections.Specialized.BitVector32;

namespace IdentityPlatform.Identity.Domain.Tokens
{
    public class RefreshTokenFamily : Entity<RefreshTokenFamily>
    {
        public Id<User> UserId { get; private set; }
        public Guid SessionId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }

        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }

        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        private RefreshTokenFamily(Id<RefreshTokenFamily> id, Id<User> userId, Guid sessionId, string ipAddress, string userAgent, TimeSpan lifetime)
         : base(id)
        {
            UserId = userId;
            SessionId = sessionId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            CreatedAt = DateTimeOffset.UtcNow;
            ExpiresAt = CreatedAt.Add(lifetime);
        }

        public static RefreshTokenFamily Create(Id<User> userId, Guid sessionId, string ipAddress, string userAgent, TimeSpan lifetime, Id<RefreshTokenFamily>? id = null)
        {
            return new RefreshTokenFamily(id ?? Id<RefreshTokenFamily>.New(), userId, sessionId, ipAddress, userAgent, lifetime);
        }

        public RefreshTokenFamily()
        {
            
        }
       
        public void Revoke()
        {
            RevokedAt = DateTimeOffset.UtcNow;
        }

        public void AddRefreshToken(RefreshToken refreshToken)
        {
            if (RevokedAt != null)
            {
                throw new InvalidOperationException("لا يمكن إضافة توكن لعائلة توكنات ملغاة.");
            }

            _refreshTokens.Add(refreshToken);
        }
    }
}
