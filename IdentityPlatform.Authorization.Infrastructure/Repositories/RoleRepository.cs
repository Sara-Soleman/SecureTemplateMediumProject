using Common.Application.Abstractions;
using Common.Domain;
using IdentityPlatform.Authorization.Application.Persistence;
using IdentityPlatform.Authorization.Domain.Roles;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using IdentityPlatform.Authorization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using System.Text;

namespace IdentityPlatform.Authorization.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AuthorizationDbContext _dbContext;
        private readonly IAuthorizationUnitOfWork _unitOfWork;

        public RoleRepository(AuthorizationDbContext dbContext ,
            IAuthorizationUnitOfWork unitOfWork)
        {

            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
           
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
            

            // 2. استخدام الكيان مباشرة في المقارنة بدلاً من الوصول إلى .Value
            return await _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_dbContext.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => r)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
           
            var rId = new Id<Role>(roleId);

            return await _dbContext.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == rId, cancellationToken);
        }

        public async Task<UserRole?> GetUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
            
            var rId = new Id<Role>(roleId);

            return await _dbContext.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == rId, cancellationToken);
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
            //await  _dbContext.Roles.AddAsync(role);
            //await _unitOfWork.SaveChangesAsync();
            

            // يمكنك طباعة هذه القيم أو وضع Breakpoint هنا لرؤية إلى أي قاعدة بيانات يوجه الاتصال فعلياً!

            await _dbContext.Roles.AddAsync(role, cancellationToken);

            // 2. جرب إجبار EF Core على تنفيذ أمر الحفظ والتحقق من عدد الصفوف المتأثرة
           // int affectedRows = await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task Update(Role role)
        {

            _dbContext.Roles.Update(role);
          
        }

        public void Remove(Role role)
        {
            _dbContext.Roles.Remove(role);
        }
    }
}
