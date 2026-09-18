using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Abstractions.Messaging
{
    public interface IIntegrationEventDispatcher
    {
        Task DispatchAsync(object integrationEvent, CancellationToken cancellationToken);
    }
}
