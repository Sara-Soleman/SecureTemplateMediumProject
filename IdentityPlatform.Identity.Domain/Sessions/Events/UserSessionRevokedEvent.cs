using Common.Domain.Events;
using IdentityPlatform.Identity.Domain.Users;
using IdentityPlatform.Identity.Domain.Users.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Sessions.Events
{
    public sealed record UserSessionRevokedEvent(
    Guid SessionId,
    Guid UserId,
    DateTimeOffset RevokedAt
) : IDomainEvent
    {
        public Guid AggregateId => SessionId;
        public string AggregateType => nameof(UserSession);
        public string EventType => nameof(UserSessionRevokedEvent);
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
        public int Version { get; init; } = 1;
        public string? TraceInfo { get; init; }
    }
    
}
