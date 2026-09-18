using Common.Application.Abstractions.Messaging;
using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Common.Infrastructure.Messaging
{
    public class KafkaIntegrationEventConsumer : IIntegrationEventConsumer, IIntegrationEventDispatcher
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly Dictionary<Type, object> _handlers;
        private readonly Dictionary<string, Type> _eventTypeMappings;

        public KafkaIntegrationEventConsumer(
            IConsumer<string, string> consumer,
            Dictionary<Type, object> handlers,
            Dictionary<string, Type> eventTypeMappings)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
            _eventTypeMappings = eventTypeMappings ?? throw new ArgumentNullException(nameof(eventTypeMappings));
        }

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = _consumer.Consume(cancellationToken);

                    if (_eventTypeMappings.TryGetValue(consumeResult.Topic, out var eventType))
                    {
                        var integrationEvent = JsonSerializer.Deserialize(consumeResult.Message.Value, eventType);

                        if (integrationEvent != null)
                        {
                            await DispatchAsync(integrationEvent, cancellationToken);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No event type mapping found for topic: {consumeResult.Topic}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during message consumption: {ex.Message}");
            }
            finally
            {
                _consumer.Close();
            }
        }

        public async Task DispatchAsync(object integrationEvent, CancellationToken cancellationToken)
        {
            var eventType = integrationEvent.GetType();

            if (_handlers.TryGetValue(eventType, out var handler))
            {
                // استخدام الـ Dynamic Invoke بطريقة آمنة ومتوافقة مع الكود السابق[cite: 4]
                var handleMethod = handler.GetType().GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    var task = (Task?)handleMethod.Invoke(handler, new object[] { integrationEvent, cancellationToken });
                    if (task != null)
                    {
                        await task;
                    }
                }
            }
            else
            {
                Console.WriteLine($"No handler found for event type: {eventType.Name}");
            }
        }
    }
}
