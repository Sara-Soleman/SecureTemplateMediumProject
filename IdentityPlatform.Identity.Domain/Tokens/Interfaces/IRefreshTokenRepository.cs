using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Tokens.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        Task AddFamilyAsync(RefreshTokenFamily refreshToken, CancellationToken cancellationToken);
        void Update(RefreshToken refreshToken);
    }
}
