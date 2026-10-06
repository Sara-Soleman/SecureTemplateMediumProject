using Common.Domain.Events;
using Common.Domain.Events.Decorators;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Authorization.Domain.Roles.Events
{
    
    [AggregateType(AuthorizationEventConstant.AuthorizationAggregateTypeName)]
    public abstract class BaseAuthorizationDomainEvent(Guid aggregateId, DateTimeOffset? occurredOnUtc = null)
        : DomainEvent(aggregateId, occurredOnUtc ?? DateTimeOffset.UtcNow)
    {
    }

}
