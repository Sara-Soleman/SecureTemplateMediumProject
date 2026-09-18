using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Core.Events
{
    /// <summary>
    /// Represents a contract for an Integration Event. 
    /// Unlike Domain Events (which stay inside a single bounded context or service), 
    /// Integration Events are published across system boundaries or microservices 
    /// to notify other independent systems that something important happened.
    /// </summary>
    public interface IIntegrationEvent
    {
        // The version number of the integration event schema, 
        // ensuring other consuming services can handle schema changes safely over time.
        int Version { get; }

        // The name or type of the integration event (e.g., "Order.CreatedForBilling").
        string EventType { get; }

        // A unique identifier (Guid) for this specific instance of the event message.
        Guid Id { get; }

        // The timestamp (in UTC) when the event occurred.
        DateTimeOffset OccurredOnUtc { get; }

        // The unique ID of the aggregate root that originally triggered the event.
        Guid AggregateId { get; }
    }
}
