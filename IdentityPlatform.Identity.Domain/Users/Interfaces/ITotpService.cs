using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Users.Interfaces
{
    public interface ITotpService
    {
        string GenerateSecretKey();
        string GetQrCodeUri(string email, string secretKey, string issuer = "IdentityPlatform");
        bool VerifyCode(string secretKey, string code);
    }
}
