using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.Application.Behaviours
{
    public sealed class ValidationPipelineBehaviour<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
        where TResponse : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehaviour(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators ?? throw new ArgumentNullException(nameof(validators));

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var results = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count > 0)
            {
                Activity.Current?.SetTag("validation.failed", true);
                Activity.Current?.SetTag("validation.error_count", failures.Count);

                var distinctProperties = failures
                    .Select(f => f.PropertyName)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct()
                    .Take(10)
                    .ToArray();

                if (distinctProperties.Length > 0)
                    Activity.Current?.SetTag("validation.properties", string.Join(",", distinctProperties));

                var messages = failures.Select(f => f.ErrorMessage).ToList();
                throw new ValidationException((IEnumerable<FluentValidation.Results.ValidationFailure>)messages);
            }

            return await next();
        }
    }
}
