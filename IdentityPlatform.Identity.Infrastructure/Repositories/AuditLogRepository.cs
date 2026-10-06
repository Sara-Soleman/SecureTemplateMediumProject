using Common.Domain;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly IdentityDbContext _context; 

        public AuditLogRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken)
        {
            await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
