using Common.Domain.Events;
using Common.Domain.Events.Decorators;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Users.Events
{
    [AggregateType(IdentityEventConstants.IdentityAggregateTypeName)]
    public abstract class BaseIdentityDomainEvent(Guid aggregateId, DateTimeOffset? occurredOnUtc = null)
        : DomainEvent(aggregateId, occurredOnUtc ?? DateTimeOffset.UtcNow)
    {
    }
    [AggregateType(IdentityEventConstants.IdentityAggregateTypeName)]
    public abstract class BaseAuthorizationDomainEvent(Guid aggregateId, DateTimeOffset? occurredOnUtc = null)
        : DomainEvent(aggregateId, occurredOnUtc ?? DateTimeOffset.UtcNow)
    {
    }

}
