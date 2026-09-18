using Common.Domain;
using IdentityPlatform.Identity.Domain.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Users.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Id<User> userId, CancellationToken cancellationToken);

        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken);

        Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken);

        Task AddAsync(User user, CancellationToken cancellationToken);

        Task<User?> GetByPasswordResetTokenAsync(string tokenHash, CancellationToken cancellationToken);


        Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken);
        Task<User?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken);

    }
}
