using Common.Domain.Events;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Messaging
{
    // محول يجعل حدث النقي (IDomainEvent) مقروءاً كـ INotification لـ MediatR دون أن يتسسخ الـ Domain
    public sealed class DomainEventNotification<TDomainEvent> : INotification
        where TDomainEvent : IDomainEvent
    {
        public TDomainEvent DomainEvent { get; }

        public DomainEventNotification(TDomainEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }
    }
}
