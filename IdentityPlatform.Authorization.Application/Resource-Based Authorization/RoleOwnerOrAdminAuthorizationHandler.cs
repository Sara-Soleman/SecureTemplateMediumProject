using IdentityPlatform.Authorization.Application.Roles.Dtos;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Resource_Based_Authorization
{

    public class RoleOwnerOrAdminAuthorizationHandler : AuthorizationHandler<SameUserOrAdminRequirement, RoleResponseDto> // أو الـ Domain Entity الخاص بك
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameUserOrAdminRequirement requirement,
            RoleResponseDto resource)
        {
            // 1. التحقق من الهوية
            if (context.User == null)
            {
                return Task.CompletedTask;
            }

            // 2. إذا كان المستخدم يملك صلاحية مطلقة (SuperAdmin)، اسمح له فوراً
            if (context.User.IsInRole("SuperAdmin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 3. القاعدة الخاصة بالموارد: منع تعديل الأدوار النظامية (System Roles الأساسية)
            if (resource.IsSystemRole) // خاصية وهمية تعبر عن أن الدور أساسي للنظام
            {
                return Task.CompletedTask; // فشل التحقق (لن يتم منح الصلاحية)
            }

            // 4. استخراج الـ UserId الحالي
            // البحث المباشر عن المعرف أو الـ NameIdentifier
            var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                ?? context.User.FindFirst("sub")?.Value; // في الـ JWT غالباً ما يكون المعرف مخزناً في "sub"
            // 5. التحقق هل المستخدم هو من أنشأ هذا الدور (إذا كان هناك حقل CreatedByUserId)
            if (resource.CreatedByUserId == currentUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
    }
