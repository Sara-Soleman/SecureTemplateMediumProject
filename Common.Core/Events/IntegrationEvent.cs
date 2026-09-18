using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Core.Events
{
    /// <summary>
    /// An abstract base implementation of the IIntegrationEvent interface.
    /// Provides default values and boilerplate setup for events that are published 
    /// across system or microservice boundaries (via message brokers like RabbitMQ or Azure Service Bus).
    /// </summary>
    public abstract class IntegrationEvent : IIntegrationEvent
    {
        // The version of the event schema. Defaults to 1 for new integration events.
        public int Version { get; set; } = 1;

        // A unique identifier for this specific event message instance.
        public Guid Id { get; set; } = Guid.NewGuid();

        // The unique identifier of the related aggregate root that triggered this event.
        public Guid AggregateId { get; set; }

        // The timestamp (in UTC) of when the integration event occurred. Defaults to the current time.
        public DateTimeOffset OccurredOnUtc { get; set; } = DateTimeOffset.UtcNow;

        // The name or type of the event (automatically set to the class name by default).
        public string EventType { get; set; }

        // Protected constructor used by specific integration events to enforce that a valid aggregate ID is provided.
        protected IntegrationEvent(Guid aggregateId)
        {
            // Defensive check: Ensures the aggregate ID is not empty, throwing an error if it is missing.
            if (aggregateId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(aggregateId), "AggregateId cannot be empty.");
            }

            AggregateId = aggregateId;

            // Automatically uses the class name of the concrete event as its event type name.
            EventType = GetType().Name;
        }

        // Default parameterless constructor required by JSON serializers and message brokers 
        // when reconstructing event objects from incoming message streams.
        protected IntegrationEvent() { }
    }
}
