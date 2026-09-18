using Common.Domain.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Abstractions.DomainEvents
{
    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
