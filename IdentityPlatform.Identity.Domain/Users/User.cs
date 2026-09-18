using Common.Domain;
using Common.Domain.Errors;
using Common.Domain.Extensions;
using CSharpFunctionalExtensions;
using IdentityPlatform.Identity.Domain.Users.Enums;
using IdentityPlatform.Identity.Domain.Users.Events;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Users
{
    public sealed class User : AggregateRoot<User>
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public AccountStatus AccountStatus { get; private set; }
        public int TokenVersion { get; private set; } = 1;

        // بيانات الاعتماد الأساسية (مرتبطة بـ Identity)
        public Credential? Credential { get; private set; }

        // بيانات استعادة كلمة المرور
        public string? PasswordResetTokenHash { get; private set; }
        public DateTimeOffset? PasswordResetTokenExpiresAt { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }


        public bool IsMfaEnabled => true; // إجباري للجميع
        public MfaType PreferredMfaType { get; private set; } = MfaType.Email; // الافتراضي أو حسب اختيار المستخدم
        public string? MfaSecret { get; private set; } // خاص بـ TOTP

        // حقول خاصة بـ Email OTP
        public string? EmailOtpCode { get; private set; }
        public DateTimeOffset? EmailOtpExpiresAt { get; private set; }

        public void SetEmailOtp(string code, TimeSpan validityDuration)
        {
            EmailOtpCode = code;
            EmailOtpExpiresAt = DateTimeOffset.UtcNow.Add(validityDuration);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public bool VerifyEmailOtp(string code)
        {
            if (string.IsNullOrEmpty(EmailOtpCode) || EmailOtpExpiresAt < DateTimeOffset.UtcNow)
                return false;

            return EmailOtpCode == code;
        }


        private User(string username, string email, string passwordHash)
        {
            Username = username.EnsureNonBlank();
            Email = email.EnsureNonBlank();

            // إنشاء بيانات الاعتماد مباشرة باستخدام معرف المستخدم المولّد تلقائياً
            Credential = Credential.Create(this.Id, passwordHash);

            AccountStatus = AccountStatus.Active;
            TokenVersion = 1;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public User()
        {
            
        }
        public static User Create(string username, string email, string passwordHash)
        {
            var user = new User(username, email, passwordHash);
            user.RaiseDomainEvent(new UserCreatedEvent(user.Id, user.Email));
            return user;
        }

        // تحديث حالة الحساب (نشط، معطل، إلخ)
        public void ChangeStatus(AccountStatus newStatus)
        {
            if (AccountStatus == newStatus)
                return;

            AccountStatus = newStatus;
            UpdatedAt = DateTimeOffset.UtcNow;

            RaiseDomainEvent(new UserStatusChangedEvent(this.Id, newStatus));
        }

        // زيادة إصدار الـ Token لإبطال الجلسات السابقة (Token Revocation)
        public void IncrementTokenVersion()
        {
            TokenVersion++;
            UpdatedAt = DateTimeOffset.UtcNow;

            RaiseDomainEvent(new TokenVersionIncrementedEvent(this.Id, TokenVersion));
        }

        // تغيير كلمة المرور باستخدام الكلاس الفرعي Credential
        public void ChangePassword(string newPasswordHash)
        {
            Credential.EnsureNonNull();
            Credential!.UpdatePasswordHash(newPasswordHash);
            UpdatedAt = DateTimeOffset.UtcNow;

            RaiseDomainEvent(new UserPasswordChangedEvent(this.Id));
        }

        // تعيين توكن استعادة كلمة المرور
        public void SetPasswordResetToken(string tokenHash, DateTimeOffset expiresAt)
        {
            PasswordResetTokenHash = tokenHash.EnsureNonBlank();
            PasswordResetTokenExpiresAt = expiresAt;
            UpdatedAt = DateTimeOffset.UtcNow;

            RaiseDomainEvent(new PasswordResetTokenGeneratedEvent(this.Id));
        }

        // تنفيذ إعادة تعيين كلمة المرور عبر التوكن
        public void ResetPassword(string newPasswordHash, string tokenHash)
        {
            Credential.EnsureNonNull();

            // التحقق من أن التوكن غير فارغ
            tokenHash.EnsureNonBlank();
            // التحقق من صحة التوكن وتاريخ الصلاحية باستخدام نمط التحقق النظيف
            var isTokenValid = (PasswordResetTokenHash == tokenHash) && (DateTimeOffset.UtcNow <= PasswordResetTokenExpiresAt);

            isTokenValid.EnsureTrue(); // إذا كان خطأ فسترمي ValidationException أو Domain Exception تلقائياً حسب تصميم المكتبة المشتركة


            Credential!.UpdatePassword(newPasswordHash);

            // مسح التوكن لأنه يُستخدم لمرة واحدة فقط
            PasswordResetTokenHash = null;
            PasswordResetTokenExpiresAt = null;
            UpdatedAt = DateTimeOffset.UtcNow;

            RaiseDomainEvent(new UserPasswordResetCompletedEvent(this.Id));
        }


        public static User CreateTestUser(string username="test", string email = "test@example.com", string passwordHash="m934fshfkaui-++-erriekjdxm")
        {
            var user = new User(username, email, passwordHash);
           
            return user;
        }

        public void Deactivate()
        {
            AccountStatus = AccountStatus.Disabled; // أو اسم الخاصية لديمو الحالة لديك
        }

        // دوال النطاق الخاصة بـ MFA
        public void SetupMfaSecret(string secret)
        {
            MfaSecret = secret.EnsureNonBlank();
            UpdatedAt = DateTimeOffset.UtcNow;
            // لا نفعله نهائياً إلا بعد تأكيد أول رمز OTP من المستخدم
        }

        public void VerifyAndEnableMfa(string secret)
        {
            MfaSecret = secret.EnsureNonBlank();
           // IsMfaEnabled = true;
            UpdatedAt = DateTimeOffset.UtcNow;

            RaiseDomainEvent(new MfaEnabledEvent(this.Id));
        }

        //public void DisableMfa()
        //{
        //    MfaSecret = null;
        //    IsMfaEnabled = false;
        //    UpdatedAt = DateTimeOffset.UtcNow;

        //    RaiseDomainEvent(new MfaDisabledEvent(this.Id));
        //}

        public Result<bool, IDomainError> ChangePreferredMfaType(MfaType newMfaType)
        {
            if (newMfaType == MfaType.Totp && string.IsNullOrEmpty(MfaSecret))
            {
                return Result.Failure<bool, IDomainError>(DomainError.InvalidMfaCode()); // أو الخطأ المناسب لديك
            }

            PreferredMfaType = newMfaType;
            return Result.Success<bool, IDomainError>(true);
        }
    }
}
