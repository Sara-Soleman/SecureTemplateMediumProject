using IdentityPlatform.Authorization.Application.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Infrastructure.Persistence
{
    public sealed class AuthorizationUnitOfWork
    : IAuthorizationUnitOfWork
    {
        private readonly AuthorizationDbContext _dbContext;

        public AuthorizationUnitOfWork(
            AuthorizationDbContext dbContext)
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
