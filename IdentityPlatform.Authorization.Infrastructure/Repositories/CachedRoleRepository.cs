using Common.Domain;
using Common.Infrastructure.Caching;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Infrastructure.Repositories
{
    public class CachedRoleRepository : IRoleRepository
    {
        private readonly IRoleRepository _decorated;
        private readonly CachedRepositoryService _cacheService; // 👈 حقن الخدمة العامة

        public CachedRoleRepository(IRoleRepository decorated, CachedRepositoryService cacheService)
        {
            _decorated = decorated;
            _cacheService = cacheService;
        }

        public async Task<IReadOnlyCollection<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            string cacheKey = $"user-roles-{userId}";

            // استخدام الخدمة العامة مباشرة بدلاً من تكرار كود الـ IMemoryCache
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                await _decorated.GetRolesByUserIdAsync(userId, cancellationToken)
            );
        }

        public async Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken)
        {
            await _decorated.AddUserRoleAsync(userRole, cancellationToken);
            _cacheService.Remove($"user-roles-{userRole.UserId}");
        }

        public void RemoveUserRole(UserRole userRole)
        {
            _decorated.RemoveUserRole(userRole);
            _cacheService.Remove($"user-roles-{userRole.UserId}"); 
        }

        // باقي الدوال تمرر للـ Repository الأصلي كالسابق...
        public async Task<Role?> GetByIdAsync(Id<Role> id, CancellationToken cancellationToken) =>
            await _decorated.GetByIdAsync(id, cancellationToken);

        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
            await _decorated.GetByNameAsync(name, cancellationToken);

        public async Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken) =>
            await _decorated.GetAllAsync(cancellationToken);

        public async Task<bool> ExistsAsync(Guid roleId, CancellationToken cancellationToken) =>
            await _decorated.ExistsAsync(roleId, cancellationToken);

        public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken) =>
            await _decorated.UserHasRoleAsync(userId, roleId, cancellationToken);

        public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken) =>
            await _decorated.GetUserRoleAsync(userId, roleId, cancellationToken);

        public async Task AddAsync(Role role, CancellationToken cancellationToken) =>
            await _decorated.AddAsync(role, cancellationToken);

        public async Task Update(Role role) => await _decorated.Update(role);

        public void Remove(Role role) => _decorated.Remove(role);
    }
}
