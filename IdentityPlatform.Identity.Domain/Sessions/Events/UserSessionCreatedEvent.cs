using Common.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Sessions.Events
{
    
    public sealed record UserSessionCreatedEvent(
    Guid SessionId,
    Guid UserId,
    string ipAddress,
    string userAgent,
    DateTimeOffset CreatedAt
) : IDomainEvent
    {
        public Guid AggregateId => SessionId;
        public string AggregateType => nameof(UserSession);
        public string EventType => nameof(UserSessionCreatedEvent);
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
        public int Version { get; init; } = 1;
        public string? TraceInfo { get; init; }
    }
}
