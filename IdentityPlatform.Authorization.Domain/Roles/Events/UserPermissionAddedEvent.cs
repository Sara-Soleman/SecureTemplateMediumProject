using Common.Domain;
using IdentityPlatform.Identity.Domain.Users.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain.Roles.Events
{
    public sealed class UserPermissionAddedEvent : BaseAuthorizationDomainEvent
    {
        public UserPermissionAddedEvent(Id<Role> roleId, string permission)
            : base(roleId.Value)
        {
            Permission = permission;
        }
        public string Permission { get; }
    }
}
