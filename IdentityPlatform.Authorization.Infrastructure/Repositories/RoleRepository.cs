using Common.Domain;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using System.Text;

namespace IdentityPlatform.Authorization.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly IdentityDbContext _dbContext;

        public RoleRepository(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Role?> GetByIdAsync(Id<Role> id, CancellationToken cancellationToken)
        {
            return await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        public async Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Roles
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid roleId, CancellationToken cancellationToken)
        {
            var id = new Id<Role>(roleId);
            return await _dbContext.Roles
                .AnyAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyCollection<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            // 1. إنشاء الكيان القوي للمعرّف خارج استعلام الـ LINQ
            var uId = new Id<User>(userId);

            // 2. استخدام الكيان مباشرة في المقارنة بدلاً من الوصول إلى .Value
            return await _dbContext.UserRoles
                .Where(ur => ur.UserId == uId)
                .Join(_dbContext.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
            var uId = new Id<User>(userId);
            var rId = new Id<Role>(roleId);

            return await _dbContext.UserRoles
                .AnyAsync(ur => ur.UserId == uId && ur.RoleId == rId, cancellationToken);
        }

        public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
            var uId = new Id<User>(userId);
            var rId = new Id<Role>(roleId);

            return await _dbContext.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == uId && ur.RoleId == rId, cancellationToken);
        }

        public async Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            await _dbContext.UserRoles.AddAsync(userRole, cancellationToken);
        }

        public void RemoveUserRole(UserRole userRole)
        {
            _dbContext.UserRoles.Remove(userRole);
        }

        public async Task AddAsync(Role role, CancellationToken cancellationToken)
        {
            await _dbContext.Roles.AddAsync(role, cancellationToken);
        }

        public void Update(Role role)
        {
            _dbContext.Roles.Update(role);
        }

        public void Remove(Role role)
        {
            _dbContext.Roles.Remove(role);
        }
    }
}
