using Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain.Roles
{
    public class UserRole : Entity<UserRole>
    {
        public Guid UserId { get; private set; }
        public Id<Role> RoleId { get; private set; }

        private UserRole(Guid userId, Id<Role> roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }

        public UserRole()
        {
            
        }
        public static UserRole Create(Guid userId, Id<Role> roleId)
        {
            return new UserRole(userId, roleId);
        }
    }
}
