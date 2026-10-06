using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Authorizations
{
    public class DynamicAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public DynamicAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // تحقق ما إذا كانت السياسة المطلوبة تخص نظام الصلاحيات الديناميكي
            if (policyName.StartsWith("Permissions."))
            {
                var policy = new AuthorizationPolicyBuilder();
                // إضافة شرط أن يحتوي المستخدم على الـ Claim الذي يمثل الصلاحية المطلوبة
                policy.AddRequirements(new PermissionRequirement(policyName));
                return policy.Build();
            }

            // إذا لم تكن تبدأ بـ Permissions، ارجع للسياسات الافتراضية العادية
            return await base.GetPolicyAsync(policyName);
        }
    }
}
