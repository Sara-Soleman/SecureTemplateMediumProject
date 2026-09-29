using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Tokens.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IdentityDbContext _dbContext;

        public RefreshTokenRepository(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
        {
            return await _dbContext.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.TokenHash == token, cancellationToken); // أو حسب حقل الـ Hash لديك
        }

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            await _dbContext.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        }

        public void Update(RefreshToken refreshToken)
        {
            _dbContext.Set<RefreshToken>().Update(refreshToken);
        }

        public async Task AddFamilyAsync(RefreshTokenFamily refreshToken, CancellationToken cancellationToken)
        {
            await _dbContext.Set<RefreshTokenFamily>().AddAsync(refreshToken, cancellationToken);
        }
    }
}
