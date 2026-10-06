using Common.Application.Messaging;
using Common.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Logging
{
    public sealed class DomainEventLoggingHandler<TEvent> : INotificationHandler<DomainEventNotification<TEvent>>
    where TEvent : IDomainEvent
    {
        private readonly ILogger<DomainEventLoggingHandler<TEvent>> _logger;

       
            public DomainEventLoggingHandler(ILogger<DomainEventLoggingHandler<TEvent>> logger)
        {
            _logger = logger;
        }

        public Task Handle(DomainEventNotification<TEvent> notification, CancellationToken cancellationToken)
        {
            // استخراج الحدث النقي من داخل المحول
            var domainEvent = notification.DomainEvent;

            // استخدام اسم الحدث ديناميكياً وتسجيله كـ Structured Log
            _logger.LogInformation(
                "SecurityDomainEvent:{EventType} | AggregateId: {AggregateId} | OccurredOn: {OccurredOn}",
                domainEvent.GetType().Name,
                domainEvent.AggregateId,
                domainEvent.OccurredOnUtc);

            return Task.CompletedTask;
        }
        
    }
}
