using Common.Core.Exceptions;
using Common.Domain.Errors;
using Common.Domain.Exceptions;
using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.Application.Behaviours
{
    public sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
        where TResponse : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (FluentValidation.ValidationException ex)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error, "validation_failed");
                Activity.Current?.AddException(ex);
                Activity.Current?.SetTag("error.type", "validation");
                Activity.Current?.SetTag("validation.error_count", ex.Errors?.Count() ?? 0);

                // هنا نأخذ الـ ErrorMessage الذي كتبناه في الـ Validator (والذي هو المفتاح مثل PasswordMissingNumber)
                var errorMessages = ex.Errors?.Select(e => e.ErrorMessage).ToList() ?? new List<string>();

                var domainError = DomainError.Validation("Validation failed", errorMessages);
                return CastOrThrow(domainError, ex);
            }
            catch (ValidationException ex)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error, "validation_failed");
                Activity.Current?.AddException(ex);
                Activity.Current?.SetTag("error.type", "validation");
                Activity.Current?.SetTag("validation.error_count", ex.Errors?.Count() ?? 0);

                var errorMessages = ex.Errors?.ToList() ?? new List<string>();

                var domainError = DomainError.Validation(ex.Message, errorMessages);
               // var domainError = DomainError.Validation(ex.Message, ex.Errors?.Select(x => x.ErrorMessage).ToList());
                return CastOrThrow(domainError, ex);
            }
            catch (myApplicationException ex)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error, "bad_request");
                Activity.Current?.AddException(ex);
                Activity.Current?.SetTag("error.type", "bad_request");

                var domainError = DomainError.BadRequest(ex.Message);
                return CastOrThrow(domainError, ex);
            }
            catch (Exception ex)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error, "unexpected");
                Activity.Current?.AddException(ex);
                Activity.Current?.SetTag("error.type", "unexpected");

                var domainError = DomainError.UnExpected("An unexpected error occurred.");
                return CastOrThrow(domainError, ex);
            }
        }

        private static TResponse CastOrThrow(IDomainError domainError, Exception ex)
        {
            //var failureResult = Result.Failure<Guid, IDomainError>(domainError);

            //if (failureResult is TResponse response)
            //{
            //    return response;
            //}

            //throw new InvalidCastException(
            //    $"Failed to cast failure result to {typeof(TResponse).Name} for request {typeof(TRequest).Name}.",
            //    ex);
            // 1. التأكد أن TResponse هو من نوع Result<TValue, IDomainError>


            var responseType = typeof(TResponse);

            // التأكد من أن TResponse هو Result<T, E>
            if (responseType.IsGenericType)
            {
                var genericArgs = responseType.GetGenericArguments();
                if (genericArgs.Length == 2)
                {
                    var valueType = genericArgs[0]; // نوع القيمة (مثل AuthResponse أو Guid)
                    var errorType = genericArgs[1]; // نوع الخطأ (IDomainError)

                    // جلب المعمّر الداخلي: Result(bool isFailure, E error, T value)
                    var internalConstructor = responseType.GetConstructor(
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                        new[] { typeof(bool), errorType, valueType });

                    if (internalConstructor != null)
                    {
                        // إذا كان نوع القيمة Struct (Value Type) ننشئ قيمة افتراضية له، وإلا نضع null
                        object defaultValue = valueType.IsValueType ? Activator.CreateInstance(valueType)! : null!;

                        // استدعاء المعمّر بوضع isFailure = true، والخطأ، والقيمة الافتراضية
                        var failureResult = internalConstructor.Invoke(new object[] { true, domainError, defaultValue });

                        if (failureResult is TResponse typedResponse)
                        {
                            return typedResponse;
                        }
                    }
                }
            }

            // احتياطي أخير في حال حدث شيء غير متوقع
            throw new InvalidCastException(
                $"Failed to create failure result for type {typeof(TResponse).Name} for request {typeof(TRequest).Name}.",
                ex);
        }
    }
}
