using Common.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Abstractions.Messaging
{
    public interface IIntegrationEventPublisher
    {
        Task PublishAsync<T>(T @event) where T : IntegrationEvent;
    }

    public interface IIntegrationEventHandler<TEvent> where TEvent : IntegrationEvent
    {
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
    }

    public interface IIntegrationEventConsumer
    {
        Task ConsumeAsync(CancellationToken cancellationToken);
    }
}
