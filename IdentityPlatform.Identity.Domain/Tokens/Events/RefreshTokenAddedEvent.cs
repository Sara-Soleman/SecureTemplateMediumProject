using Common.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Tokens.Events
{

    public sealed record RefreshTokenAddedEvent(
Guid FamilyId,
Guid NewTokenId,
DateTimeOffset AddedAt
) : IDomainEvent
    {
        public Guid AggregateId => FamilyId;
        public string AggregateType => nameof(RefreshTokenFamily);
        public string EventType => nameof(RefreshTokenAddedEvent);
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
        public int Version { get; init; } = 1;
        public string? TraceInfo { get; init; }
    }
}