using IdentityPlatform.Authorization.Domain;
using IdentityPlatform.Authorization.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Infrastructure.Persistence
{
    public class RoleSeeder
    {
        private readonly AuthorizationDbContext _context;

        public RoleSeeder(AuthorizationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // 1. التحقق إن كان دور الـ Admin موجوداً مسبقاً
            if (!await _context.Roles.AnyAsync(r => r.Name == "Admin"))
            {
                // 2. إنشاء دور Admin وإعطائه كافة الصلاحيات الموجودة في ملف Permissions
                var allPermissions = Permissions.GetAllPermissions().ToList();

                var adminRole = Role.Create(
                    name: "Admin",
                    description: "Administrator Role with full permissions",
                    permissions: allPermissions // تمرير الصلاحيات هنا لتخزينها في قاعدة البيانات
                );

                await _context.Roles.AddAsync(adminRole);
                await _context.SaveChangesAsync();
            }
        }
    }
}
