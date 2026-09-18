using Common.Domain.Events;
using Common.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain
{
    /// <summary>
    /// Represents an Aggregate Root in Domain-Driven Design (DDD). 
    /// An aggregate root is the main entry point for a cluster of associated objects (an aggregate) 
    /// and is responsible for tracking significant business occurrences, known as domain events.
    /// </summary>
    public interface IAggregateRoot
    {
        // A read-only collection of domain events that have occurred within this aggregate.
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        // Retrieves all currently recorded domain events and immediately clears them from the aggregate 
        // (commonly used when saving changes to dispatch these events to the rest of the application).
        IReadOnlyCollection<IDomainEvent> PopDomainEvents();

        // Clears all domain events without returning them.
        void ClearEvents();
    }

    /// <summary>
    /// An abstract base class for aggregate roots. It inherits from your Entity class 
    /// and implements IAggregateRoot. The generic constraint 'where TModel : IAuditableEntity' 
    /// ensures that the model type associated with this aggregate can also track its audit history.
    /// </summary>
    public abstract class AggregateRoot<TModel> : Entity<TModel>, IAggregateRoot
        where TModel : IAuditableEntity
    {
        // A private list that stores domain events internally. 
        // Using '[]' is a concise C# syntax for initializing a new empty list.
        private readonly IList<IDomainEvent> _domainEvents = [];

        // Exposes the private list as a public, read-only collection. 
        // This prevents outside code from modifying the events list directly, protecting encapsulation.
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        // Grabs a copy of all current events, clears the internal list, and returns the events 
        // so they can be processed (e.g., sent to a message broker or event handler).
        public IReadOnlyCollection<IDomainEvent> PopDomainEvents()
        {
            var events = _domainEvents.ToList();
            ClearEvents();
            return events;
        }

        // Wipes out all stored domain events.
        public void ClearEvents()
        {
            _domainEvents.Clear();
        }

        // A protected helper method that derived classes (specific aggregate implementations) 
        // can call to record a new business event when something important happens.
        protected void RaiseDomainEvent(IDomainEvent domainEvent)
        {
            // Ensures the event object isn't null before adding it.
            domainEvent.EnsureNonNull();
            _domainEvents.Add(domainEvent);
        }
    }
}
