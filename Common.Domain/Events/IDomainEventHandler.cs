
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Domain.Events
{
    /// <summary>
    /// A custom interface for handling domain events, acting as a wrapper around MediatR's INotificationHandler.
    /// Any class that implements this interface will be responsible for executing side effects or business logic 
    /// when a specific domain event occurs (e.g., sending a welcome email when a UserRegistered event happens).
    /// </summary>
    /// <typeparam name="TDomainEvent">The specific type of domain event this handler is designed to process.</typeparam>
    public interface IDomainEventHandler<TDomainEvent> 
        where TDomainEvent : IDomainEvent
    {
        // This interface body is empty because it doesn't need to define any new methods. 
        // Instead, it inherits everything it needs from MediatR's INotificationHandler.
    }
}


