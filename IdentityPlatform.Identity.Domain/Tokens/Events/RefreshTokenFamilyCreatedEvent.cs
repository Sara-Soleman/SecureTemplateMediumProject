using Common.Domain.Events;
using IdentityPlatform.Identity.Domain.Sessions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Tokens.Events
{
    public sealed record RefreshTokenFamilyCreatedEvent(
    Guid FamilyId,
    Guid UserId,
    Guid SessionId,
    DateTimeOffset CreatedAt
) : IDomainEvent
    {
        public Guid AggregateId => FamilyId;
        public string AggregateType => nameof(RefreshTokenFamily);
        public string EventType => nameof(RefreshTokenFamilyCreatedEvent);
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
        public int Version { get; init; } = 1;
        public string? TraceInfo { get; init; }
    }
    
}
