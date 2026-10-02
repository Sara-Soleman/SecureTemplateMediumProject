using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain
{
    /// <summary>
    /// لتجنب الوقوع في خطأ الـ Magic Strings وتوحيد أسماء الصلاحيات في جميع أنحاء النظام (الـ API، الـ Database، والـ Frontend)
    /// سننشئ كلاس ثابت يجمع كل صلاحيات النظام
    /// </summary>
    public static class Permissions
    {
        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Update = "Permissions.Users.Update";
            public const string Delete = "Permissions.Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Manage = "Permissions.Roles.Manage";
        }

        public static class Audit
        {
            public const string ViewLogs = "Permissions.Audit.ViewLogs";
        }

        /// <label>استرجاع كافة الصلاحيات المتاحة في النظام (مفيد للـ Seeding والتحقق)</label>
        public static IReadOnlyCollection<string> GetAllPermissions()
        {
            return new List<string>
            {
                Users.View,
                Users.Create,
                Users.Update,
                Users.Delete,
                Roles.View,
                Roles.Manage,
                Audit.ViewLogs
            };
        }
    }
}
