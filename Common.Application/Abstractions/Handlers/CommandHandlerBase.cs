using Common.Application.Abstractions.CQRS;
using Common.Application.Abstractions.DomainEvents;
using Common.Domain;
using Common.Domain.Errors;
using Common.Domain.Events;
using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Application.Abstractions.Handlers
{
    public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
     where TCommand : ICommand<TResponse>
     where TResponse : notnull
    {
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IUnitOfWork _unitOfWork;

        protected CommandHandlerBase(
            IDomainEventDispatcher domainEventDispatcher,
            IUnitOfWork unitOfWork)
        {
            _domainEventDispatcher = domainEventDispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TResponse, IDomainError>> Handle(TCommand request, CancellationToken cancellationToken)
        {
            var operationResult = await ExecuteAsync(request, cancellationToken);
            if (!operationResult.IsSuccess)
            {
                return operationResult;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var aggregateRoot = GetAggregateRoot(operationResult);
            if (aggregateRoot != null)
            {
                var domainEvents = aggregateRoot.PopDomainEvents();
                await DispatchDomainEventsAsync(domainEvents, cancellationToken);
            }

            return operationResult;
        }

        protected abstract Task<Result<TResponse, IDomainError>> ExecuteAsync(TCommand request, CancellationToken cancellationToken);
        protected abstract IAggregateRoot? GetAggregateRoot(Result<TResponse, IDomainError> result);

        protected async Task DispatchDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            if (domainEvents == null) return;
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }
    }
}

