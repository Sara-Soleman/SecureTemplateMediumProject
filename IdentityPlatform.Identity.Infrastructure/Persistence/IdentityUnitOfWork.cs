using IdentityPlatform.Identity.Application.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence
{
    public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
    {
        private readonly IdentityDbContext _dbContext;

        public IdentityUnitOfWork(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
