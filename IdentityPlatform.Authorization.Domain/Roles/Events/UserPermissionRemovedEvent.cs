using Common.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain.Roles.Events
{
    public sealed class UserPermissionRemovedEvent : BaseAuthorizationDomainEvent
    {
        public UserPermissionRemovedEvent(Id<Role> roleId, string permission)
            : base(roleId.Value)
        {
            Permission = permission;
        }
        public string Permission { get; }
    }
    
}
