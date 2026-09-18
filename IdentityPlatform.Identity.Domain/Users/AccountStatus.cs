using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Users
{
    public enum AccountStatus
    {
        Pending = 0,   // قيد الانتظار أو التفعيل
        Active = 1,    // مفعل ونشط
        Disabled = 2,  // معطل
        Locked = 3     // مقفل (بسبب محاولات دخول خاطئة مثلاً)
    }
}
