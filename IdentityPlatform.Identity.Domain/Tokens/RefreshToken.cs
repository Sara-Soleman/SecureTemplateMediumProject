using Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Tokens
{
    public class RefreshToken : Entity<RefreshToken>
    {
        public Id<RefreshTokenFamily> FamilyId { get; private set; }
        public string TokenHash { get; private set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset? ConsumedAt { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }
        public Id<RefreshToken>? ReplacedByTokenId { get; private set; }

        public RefreshTokenFamily Family { get; private set; } = null!;

        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }


        private RefreshToken(Id<RefreshToken> id, Id<RefreshTokenFamily> familyId, string tokenHash, TimeSpan lifetime)
         : base(id)
        {
            FamilyId = familyId;
            TokenHash = tokenHash;
            ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime);
        }

        public static RefreshToken Create(Id<RefreshTokenFamily> familyId, string tokenHash, TimeSpan lifetime, Id<RefreshToken>? id = null)
        {
            return new RefreshToken(id ?? Id<RefreshToken>.New(), familyId, tokenHash, lifetime);
        }
        public RefreshToken()
        {
            
        }


        public void Consume(Id<RefreshToken> replacingTokenId)
        {
            ConsumedAt = DateTimeOffset.UtcNow;
            ReplacedByTokenId = replacingTokenId;
        }

        public void Revoke()
        {
            RevokedAt = DateTimeOffset.UtcNow;
        }

        public void SetFamily(RefreshTokenFamily family)
        {
            Family = family ?? throw new ArgumentNullException(nameof(family));
        }
    }
}
