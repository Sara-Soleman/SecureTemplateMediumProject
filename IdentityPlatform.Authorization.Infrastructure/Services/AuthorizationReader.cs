
using IdentityPlatform.Authorization.Application.Roles.Dtos;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using IdentityPlatform.Authorization.Domain.Roles.Interfaces;
using IdentityPlatform.Identity.Application.Authorization;

namespace IdentityPlatform.Authorization.Infrastructure.Services
{
    public class AuthorizationReader : IAuthorizationReader
    {
        private readonly IRoleRepository _roleRepository;

        public AuthorizationReader(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<UserAuthorizationData> GetUserAuthorizationDataAsync(Guid userId, CancellationToken cancellationToken)
        {
            // استعلام مباشر وسريع داخل سياق الصلاحيات الخاص به
            var roles = await _roleRepository.GetRolesByUserIdAsync(userId, cancellationToken);

            var roleNames = roles.Select(r => r.Name).ToList();
            var permissions = roles.SelectMany(r => r.Permissions).Distinct().ToList();

            return new UserAuthorizationData(roleNames, permissions);
        }
    }
}
