using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid? UserId { get; set; }        // من قام بالعملية
        public string ActionName { get; set; }   // اسم الـ Command (مثل AssignRoleToUserCommand)
        public string Parameters { get; set; }   // تفاصيل الطلب (JSON)
        public bool IsSuccess { get; set; }      // هل نجحت العملية أم فشلت؟
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
