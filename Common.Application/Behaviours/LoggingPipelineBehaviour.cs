using Common.Application.Observability;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.Application.Behaviours
{
    public sealed class LoggingPipelineBehaviour<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private static readonly TimeSpan SlowCommandThreshold =
            TimeSpan.FromSeconds(5);

        private readonly ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> _logger;

        public LoggingPipelineBehaviour(
            ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var commandName = typeof(TRequest).Name;

            using var activity = CommonActivitySource.Instance.StartActivity(
                commandName,
                ActivityKind.Internal);

            activity?.SetTag("messaging.system", "mediatr");
            activity?.SetTag("command.name", commandName);
            activity?.SetTag("layer", "application");

            _logger.LogInformation(
                "Command started: {CommandName}",
                commandName);

            var startTimestamp = Stopwatch.GetTimestamp();

            try
            {
                var response = await next();

                var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

                if (elapsed > SlowCommandThreshold)
                {
                    _logger.LogWarning(
                        "Slow command detected: {CommandName}. Duration: {DurationMs} ms",
                        commandName,
                        elapsed.TotalMilliseconds);
                }

                activity?.SetStatus(ActivityStatusCode.Ok);

                _logger.LogInformation(
                    "Command completed: {CommandName}. Duration: {DurationMs} ms",
                    commandName,
                    elapsed.TotalMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                var elapsed = Stopwatch.GetElapsedTime(startTimestamp);

                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);

                _logger.LogError(
                    ex,
                    "Command failed: {CommandName}. Duration: {DurationMs} ms",
                    commandName,
                    elapsed.TotalMilliseconds);

                throw;
            }
        }
    }
}
