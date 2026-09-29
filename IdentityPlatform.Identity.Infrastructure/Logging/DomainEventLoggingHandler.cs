using Common.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityPlatform.Identity.Infrastructure.Logging
{
    public sealed class DomainEventLoggingHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IDomainEvent
    {
        private readonly ILogger<DomainEventLoggingHandler<TEvent>> _logger;

       
            public DomainEventLoggingHandler(ILogger<DomainEventLoggingHandler<TEvent>> logger)
        {
            _logger = logger;
        }
        public Task Handle(TEvent notification, CancellationToken cancellationToken)
        {
            // استخدام اسم الحدث ديناميكياً وتسجيله كـ Structured Log
            _logger.LogInformation(
                "SecurityDomainEvent:{EventType} | AggregateId: {AggregateId} | OccurredOn: {OccurredOn}",
                typeof(TEvent).Name,
                notification.AggregateId,
                notification.OccurredOnUtc);

            return Task.CompletedTask;
        }
    }
}
