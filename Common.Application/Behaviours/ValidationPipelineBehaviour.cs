using Common.Domain.Errors;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

using Validation = Common.Domain.Exceptions;

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
                var errorMessages = failures.Select(f => f.ErrorMessage).Distinct().ToList();
                var domainError = DomainError.Validation("Validation failed", errorMessages);

                // التحقق مما إذا كان TResponse هو Result<TValue, IDomainError>
                var responseType = typeof(TResponse);
                if (responseType.IsGenericType)
                {
                    var errorType = responseType.GetGenericArguments()[1]; // IDomainError

                    // البحث عن دالة الـ Failure في الSTRUCT الخاص بك وإنشاؤها مباشرة
                    var failureMethod = responseType.GetMethod("Failure", new[] { errorType });
                    if (failureMethod != null)
                    {
                        var failureResult = failureMethod.Invoke(null, new object[] { domainError });
                        if (failureResult is TResponse typedResponse)
                        {
                            return typedResponse; // <-- إرجاع النتيجة الفاشلة مباشرة دون رمي أي Exception!
                        }
                    }
                }


                //Activity.Current?.SetTag("validation.failed", true);
                //Activity.Current?.SetTag("validation.error_count", failures.Count);

                //var distinctProperties = failures
                //    .Select(f => f.PropertyName)
                //    .Where(p => !string.IsNullOrWhiteSpace(p))
                //    .Distinct()
                //    .Take(10)
                //    .ToArray();

                //if (distinctProperties.Length > 0)
                //    Activity.Current?.SetTag("validation.properties", string.Join(",", distinctProperties));

                //var messages = failures.Select(f => f.ErrorMessage).ToList();
                throw new Validation.ValidationException(errorMessages);
            }

            return await next();
        }
    }
}
