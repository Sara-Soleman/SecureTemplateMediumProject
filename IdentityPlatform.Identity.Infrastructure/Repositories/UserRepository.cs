using Common.Domain;
using IdentityPlatform.Identity.Domain.Tokens;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Interfaces;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _context;

        public UserRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Id<User> userId, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Credential) // جلب بيانات الاعتماد مع المستخدم
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Credential)
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail, cancellationToken);
        }

        public async Task<bool> ExistsByUsernameOrEmailAsync(string username, string email, CancellationToken cancellationToken)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username || u.Email == email, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        public async Task<User?> GetByPasswordResetTokenAsync(string tokenHash, CancellationToken cancellationToken)
        {
            return await _context.Users
                .Include(u => u.Credential)
                .FirstOrDefaultAsync(u => u.PasswordResetTokenHash == tokenHash &&
                                          u.PasswordResetTokenExpiresAt > DateTimeOffset.UtcNow,
                                     cancellationToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.Family)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
        }



        public async Task<User?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var family = await _context.RefreshTokenFamilies
                .FirstOrDefaultAsync(f => f.SessionId == sessionId, cancellationToken);

            if (family == null)
            {
                return null;
            }

            // بما أن family.UserId من نوع Id<User> و u.Id نفس النوع، ستتم المطابقة بسلاسة تامة
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == family.UserId, cancellationToken);
        }
    }
}
