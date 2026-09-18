using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Sessions.Interfaces
{
    public interface IUserSessionRepository
    {
        Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);
        Task<UserSession?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
        Task<IEnumerable<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task AddAsync(UserSession session, CancellationToken cancellationToken);
        void Update(UserSession session);
    }
}
