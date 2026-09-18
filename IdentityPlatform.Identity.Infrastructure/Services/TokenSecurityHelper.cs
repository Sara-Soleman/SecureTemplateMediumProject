using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Services
{
    public static class TokenSecurityHelper
    {
        public static string GenerateSecureTokenString()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static string HashToken(string token)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(token);
            var hashBytes = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
