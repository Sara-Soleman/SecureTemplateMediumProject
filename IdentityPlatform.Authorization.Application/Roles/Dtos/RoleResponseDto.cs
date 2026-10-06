using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Application.Roles.Dtos
{
    public class RoleResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // 👈 خصائص أساسية للتحقق من المورد (Resource-Based Authorization)
        public bool IsSystemRole { get; set; }        // هل الدور يتبع للنظام ولا يمكن تعديله؟
        public string? CreatedByUserId { get; set; }   // معرف المستخدم الذي أنشأ هذا الدور
    }
}
