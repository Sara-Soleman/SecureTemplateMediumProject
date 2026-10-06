using Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Authorizations
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;
        // 

        public PermissionAuthorizationHandler(ICurrentUserService currentUserService, ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // 1. التحقق من أن المستخدم مسجل الدخول
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Authorization failed: User is not authenticated.");
                return Task.CompletedTask;
            }
            _logger.LogInformation("--- Checking Permissions for User: {User} ---", context.User.Identity.Name);
            foreach (var claim in context.User.Claims)
            {
                _logger.LogInformation("Claim Type: {Type}, Claim Value: {Value}", claim.Type, claim.Value);
            }

            _logger.LogInformation("Required Permission Policy: {Requirement}", requirement.Permission);
            //// 2. فحص هل الـ User يمتلك الـ Claim الخاص بالصلاحية المطلوبة
            //// (ملاحظة: عندما يصدر الـ JWT Token، يتم تضمين الصلاحيات بداخله كـ Claims)
            //var hasPermission = context.User.Claims.Any(c => c.Type == "Permission" && c.Value == requirement.Permission);

            //if (hasPermission)
            //{
            //    context.Succeed(requirement);
            //}

            var hasPermission = context.User.Claims
            .Any(c => (c.Type.Equals("permission", System.StringComparison.OrdinalIgnoreCase)
                       || c.Type.EndsWith("permission", System.StringComparison.OrdinalIgnoreCase))
                      && c.Value.Equals(requirement.Permission, System.StringComparison.OrdinalIgnoreCase));

            if (hasPermission)
            {
                _logger.LogInformation("Permission matched successfully!");
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("Permission denied for requirement: {Requirement}", requirement.Permission);
            }

            return Task.CompletedTask;
        }
    }
}
