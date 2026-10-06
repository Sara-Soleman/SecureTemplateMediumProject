using Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain.Roles.Interfaces
{
    public interface IRoleRepository
    {
        // عمليات الاستعلام الأساسية للأدوار
        Task<Role?> GetByIdAsync(Id<Role> id, CancellationToken cancellationToken);
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<Role>> GetAllAsync(CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid roleId, CancellationToken cancellationToken);

        // استرجاع الأدوار المرتبطة بمستخدم معين
        Task<IReadOnlyCollection<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        // عمليات التحقق والإدارة الخاصة بعلاقة المستخدم بالدور (UserRoles)
        Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);
        Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken);

        Task AddUserRoleAsync(UserRole userRole, CancellationToken cancellationToken);
        void RemoveUserRole(UserRole userRole);

        // عمليات الإدارة العامة للأدوار (Add, Update, Remove)
        Task AddAsync(Role role, CancellationToken cancellationToken);
        Task Update(Role role);
        void Remove(Role role);
    }
}
