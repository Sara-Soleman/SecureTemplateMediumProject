using Common.Domain;
using IdentityPlatform.Identity.Domain.Users.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain.Roles.Events
{
    public sealed class UserPermissionUpdatedEvent : BaseAuthorizationDomainEvent
    {
        public UserPermissionUpdatedEvent(Id<Role> roleId, string name, string description)
            : base(roleId.Value)
        {
            Name = name;
            Description = description;
        }
        public string Description { get; }
        public string Name { get; }

    }
    
}
