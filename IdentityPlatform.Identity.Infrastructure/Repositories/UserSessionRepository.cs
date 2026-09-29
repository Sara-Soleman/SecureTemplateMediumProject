using Common.Domain;
using IdentityPlatform.Identity.Domain.Sessions;
using IdentityPlatform.Identity.Domain.Sessions.Interfaces;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Repositories
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly IdentityDbContext _dbContext; // أو استبدل DbContext باسم الـ DbContext الفعلي لمشروعك (مثل ApplicationDbContext)

        public UserSessionRepository(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<UserSession>()
             .FirstOrDefaultAsync(s => s.Id.Equals(sessionId), cancellationToken);
        }

        public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<UserSession>()
                .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);
        }

        public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var userId1 = new Id<User>(userId);
            return await _dbContext.Set<UserSession>()
                .Where(s => s.UserId == userId1 && s.RevokedAt == null && s.ExpiresAt > now)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserSession session, CancellationToken cancellationToken)
        {
            await _dbContext.Set<UserSession>().AddAsync(session, cancellationToken);
        }

        public void Update(UserSession session)
        {
            _dbContext.Set<UserSession>().Update(session);
        }
    }
}
