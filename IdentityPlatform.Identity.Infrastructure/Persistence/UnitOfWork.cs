using Common.Application.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Persistence
{
    public class UnitOfWork(IdentityDbContext dbContext) : IUnitOfWork
    {
        private readonly IdentityDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Save changes to the database
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose() => _dbContext.Dispose();
    }

}
