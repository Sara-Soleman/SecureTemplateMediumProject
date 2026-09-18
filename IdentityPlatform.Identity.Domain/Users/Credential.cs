using Common.Domain;
using Common.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using IdentityPlatform.Identity.Domain.Users;
namespace IdentityPlatform.Identity.Domain.Users
{
    public sealed class Credential : Entity<Credential>
    {
        public Id<User> UserId { get; private set; }
        public string PasswordHash { get; private set; }
        public DateTimeOffset PasswordChangedAt { get; private set; }
        public int PasswordVersion { get; private set; }

        // Navigation Property للربط العكسي مع المستخدم
        public User User { get; private set; } = null!;

        public Credential()
        {
            
        }
        // Constructor خاص
        private Credential(Id<Credential> id, Id<User> userId, string passwordHash)
            : base(id)
        {
            UserId = userId.EnsureNonNull();
            PasswordHash = passwordHash.EnsureNonBlank();
            PasswordChangedAt = DateTimeOffset.UtcNow;
            PasswordVersion = 1;
        }

        // دالة مصنع ثابتة تستخدم Id<Credential> مباشرة مثل الـ BasketItem
        public static Credential Create(Id<User> userId, string passwordHash, Id<Credential>? id = null)
        {
            return new Credential(id ?? Id<Credential>.New(), userId, passwordHash);
        }

        public void UpdatePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash.EnsureNonBlank();
            PasswordChangedAt = DateTimeOffset.UtcNow;
            PasswordVersion++;
        }

        public void UpdatePasswordHash(string newPasswordHash)
        {
            PasswordHash = newPasswordHash.EnsureNonBlank();
            PasswordChangedAt = DateTimeOffset.UtcNow;
        }
    }
}
