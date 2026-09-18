using Common.Domain.Events.Decorators;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Common.Domain.Events
{
    /// <summary>
    /// A base implementation of the IDomainEvent interface. 
    /// It provides default values (like unique IDs and version numbers) and handy helper methods 
    /// to automatically derive event names and metadata using C# reflection.
    /// </summary>
    public class DomainEvent : IDomainEvent
    {
        // The version of the event schema. Defaults to 1 for new events.
        public int Version { get; set; } = 1;

        // A unique identifier for this specific event instance, automatically generated as a new Guid.
        public Guid Id { get; set; } = Guid.NewGuid();

        // The ID of the aggregate root that triggered this event.
        public Guid AggregateId { get; set; }

        // The timestamp (in UTC) of when the event happened.
        public DateTimeOffset OccurredOnUtc { get; set; }

        // The name of the event type (e.g., "Order.OrderPlaced").
        public string EventType { get; set; }

        // The name of the aggregate type that owns this event (e.g., "Order").
        public string AggregateType { get; set; }

        // Optional debugging or tracing information (like a correlation ID for logs).
        public string? TraceInfo { get; set; }

        // Default constructor (required by many serializers when reading events from a database or message broker).
        public DomainEvent() { }

        // Protected constructor used by specific, custom domain events (e.g., OrderPlacedEvent) 
        // to ensure required data and automatic metadata are always populated.
        protected DomainEvent(Guid aggregateId, DateTimeOffset occurredOnUtc)
        {
            // Ensure the aggregate ID is valid (not empty), throwing an exception if it is missing.
            AggregateId = aggregateId != Guid.Empty ? aggregateId : throw new ArgumentNullException(nameof(aggregateId));
            OccurredOnUtc = occurredOnUtc;

            // Automatically find the AggregateType using reflection and attributes on the event class.
            AggregateType = GetAggregateType(GetType()) ?? throw new InvalidOperationException("Aggregate type cannot be null.");

            // Automatically generate a clean, standard EventType name.
            EventType = GetEventType(this);
        }

        // --- Reflection Helper Methods ---
        // These static methods use C# Reflection to inspect event classes at runtime 
        // so you don't have to manually hardcode event names or types everywhere.

        // Looks up the 'AggregateTypeAttribute' attached to an event class.
        public static string GetAggregateType<TEvent>() where TEvent : IDomainEvent =>
            GetAggregateType(typeof(TEvent));

        public static string GetAggregateType(Type eventType)
        {
            var attribute = eventType.GetCustomAttribute<AggregateTypeAttribute>();
            return attribute?.AggregateType ?? string.Empty;
        }

        // Combines the aggregate prefix and class name to create a full EventType (e.g., "Order.OrderPlaced").
        public static string GetEventType(IDomainEvent @event) =>
            GetEventType(@event.GetType(), @event.AggregateType);

        public static string GetEventType<TEvent>() where TEvent : IDomainEvent =>
            GetEventType(typeof(TEvent));

        public static string GetEventType(Type eventType, string? prefix = null)
        {
            prefix ??= GetAggregateType(eventType);
            return $"{prefix}.{eventType.Name}";
        }
    }
}
