using Common.Domain;
using IdentityPlatform.Identity.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain.Roles
{
    public class UserRole : Entity<UserRole>
    {
        public Id<User> UserId { get; private set; }
        public Id<Role> RoleId { get; private set; }

        private UserRole(Id<User> userId, Id<Role> roleId)
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
