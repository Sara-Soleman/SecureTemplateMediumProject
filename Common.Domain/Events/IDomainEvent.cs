using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Events
{
    /// <summary>
    /// Represents a contract for a Domain Event. 
    /// It inherits from 'INotification' (commonly used with libraries like MediatR), 
    /// meaning any implementing event can be easily published and handled asynchronously across the application.
    /// </summary>
    public interface IDomainEvent : INotification
    {
        // The version number of the event schema. 
        // This helps manage backward compatibility if the structure of the event changes in the future.
        int Version { get; }

        // The name or type of the aggregate root that raised this event (e.g., "Order" or "Customer").
        string AggregateType { get; }

        // The specific name of the event itself (e.g., "OrderPlaced" or "CustomerEmailChanged").
        string EventType { get; }

        // A unique identifier (Guid) for this specific instance of the event.
        Guid Id { get; }

        // The exact timestamp (in UTC) when the event occurred in the domain.
        DateTimeOffset OccurredOnUtc { get; }

        // The unique ID of the specific aggregate root that triggered this event.
        Guid AggregateId { get; }

        // Optional tracking or tracing information (like a correlation ID or request ID) 
        // used for logging, debugging, and tracing requests across services.
        string? TraceInfo { get; }
    }
}