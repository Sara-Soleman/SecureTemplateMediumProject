using Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Users.Events
{
    // حدث إنشاء مستخدم جديد
    public sealed class UserCreatedEvent : BaseIdentityDomainEvent
    {
        public UserCreatedEvent(Id<User> userId, string email)
            : base(userId.Value)
        {
            Email = email;
        }

        public string Email { get; }
    }


    // حدث تغيير حالة الحساب
    public sealed class UserStatusChangedEvent : BaseIdentityDomainEvent
    {
        public UserStatusChangedEvent(Id<User> userId, AccountStatus newStatus)
            : base(userId.Value)
        {
            NewStatus = newStatus;
        }

        public AccountStatus NewStatus { get; }
    }

    // حدث زيادة إصدار التوكن لإبطال الجلسات
    public sealed class TokenVersionIncrementedEvent : BaseIdentityDomainEvent
    {
        public TokenVersionIncrementedEvent(Id<User> userId, int newTokenVersion)
            : base(userId.Value)
        {
            NewTokenVersion = newTokenVersion;
        }

        public int NewTokenVersion { get; }
    }

    // حدث تغيير كلمة المرور
    public sealed class UserPasswordChangedEvent : BaseIdentityDomainEvent
    {
        public UserPasswordChangedEvent(Id<User> userId)
            : base(userId.Value)
        {
        }
    }

    // حدث توليد توكن استعادة كلمة المرور
    public sealed class PasswordResetTokenGeneratedEvent : BaseIdentityDomainEvent
    {
        public PasswordResetTokenGeneratedEvent(Id<User> userId)
            : base(userId.Value)
        {
        }
    }

    // حدث إتمام إعادة تعيين كلمة المرور بنجاح
    public sealed class UserPasswordResetCompletedEvent : BaseIdentityDomainEvent
    {
        public UserPasswordResetCompletedEvent(Id<User> userId)
            : base(userId.Value)
        {
        }
    }
}
