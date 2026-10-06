using Common.Domain;
using Common.Domain.Events.Decorators;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain.Roles.Events
{
    [AggregateType(AuthorizationEventConstant.AuthorizationAggregateTypeName)]
    public sealed class RoleCreatedEvent : BaseAuthorizationDomainEvent
    {
        public RoleCreatedEvent(Id<Role> roleId, string name, string description)
            : base(roleId.Value)
        {
            Name = name;
            Description = description;
        }
        public string Name { get; }
        public string Description { get; }
    }
    
    
}
