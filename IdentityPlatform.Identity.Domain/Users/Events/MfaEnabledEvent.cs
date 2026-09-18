using Common.Domain.Events;
using IdentityPlatform.Identity.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Domain.Users.Events
{
    public sealed record MfaEnabledEvent(Guid UserId) : IDomainEvent
    {
        public Guid AggregateId => UserId;
        public string AggregateType => nameof(User);
        public string EventType => nameof(MfaEnabledEvent);
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;
        public int Version { get; init; } = 1;
        public string? TraceInfo { get; init; }
    }
}
