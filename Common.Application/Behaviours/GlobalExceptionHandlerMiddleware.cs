using Common.Domain.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Common.Application.Behaviours
{
    public sealed class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "حدث استثناء غير معالج في الـ Pipeline: {Message}", exception.Message);

                await HandleExceptionAsync(context, exception);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            

            IDomainError domainError = exception switch
            {
                IDomainError domainEx => DomainError.Validation("Domain.Validation", new List<string> { domainEx.ErrorMessage }),
                UnauthorizedAccessException => DomainError.Unauthorized(),
                KeyNotFoundException => DomainError.NotFound(),
                _ => DomainError.UnExpected("حدث خطأ غير متوقع في الخادم.")
            };

            // تعيين كود الحالة (Status Code) بناءً على نوع الخطأ
            context.Response.StatusCode = domainError.ErrorType.Name switch
            {
                nameof(ErrorType.Validation) => StatusCodes.Status400BadRequest,
                nameof(ErrorType.Unauthorized) => StatusCodes.Status401Unauthorized,
                nameof(ErrorType.Forbidden) => StatusCodes.Status403Forbidden,
                nameof(ErrorType.NotFound) => StatusCodes.Status404NotFound,
                nameof(ErrorType.Conflict) => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            // يمكنك إرجاع كائن الـ DomainError مباشرة ليكون شكل الـ JSON موحداً تماماً مع باقي الـ API
            var response = JsonSerializer.Serialize(new
            {
                isSuccess = false,
                error = domainError
            });

            await context.Response.WriteAsync(response);
        }
    }
}
