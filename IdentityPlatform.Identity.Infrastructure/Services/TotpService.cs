using IdentityPlatform.Identity.Domain.Users.Interfaces;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Services
{
    public class TotpService : ITotpService
    {
        public string GenerateSecretKey()
        {
            var secretKey = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(secretKey);
        }

        public string GetQrCodeUri(string email, string secretKey, string issuer = "IdentityPlatform")
        {
            // صيغة الـ URI الخاصة بتطبيقات المصادقة مثل Google Authenticator
            return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}&digits=6&period=30";
        }

        public bool VerifyCode(string secretKey, string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
                return false;

            var bytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(bytes);

            // التحقق من الرمز مع السماح بفارق زمن بسيط جداً (Window) لتجنب مشاكل فارق توقيت الجهاز
            return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
    }
}
