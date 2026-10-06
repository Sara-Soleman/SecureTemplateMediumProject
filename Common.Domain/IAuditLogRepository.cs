using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken);
    }
}
