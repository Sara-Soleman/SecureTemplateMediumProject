using Common.Application.Observability;
using Common.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common.Application.Behaviours
{
    public sealed class CommandMetricsBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
        where TResponse : notnull
    {
        private readonly ILogger<CommandMetricsBehavior<TRequest, TResponse>> _logger;  //في حال اردت طباعة هذه المقاييس في ملفات الوغ لانه ليس لدي برامج لمعالجتها ورؤية المقاييس 
        public CommandMetricsBehavior(ILogger<CommandMetricsBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var commandName = typeof(TRequest).Name;
            var startTimestamp = Stopwatch.GetTimestamp();

            CommandMetrics.Attempts.Add(1,
                new TagList
                {
                    { "command", commandName }
                });

            try
            {
                var response = await next();

                CommandMetrics.Success.Add(1,
                    new TagList
                    {
                        { "command", commandName }
                    });

                RecordDuration(commandName, "success", startTimestamp);
                var elapsedSeconds = Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds;
                _logger.LogInformation("[Metrics] Command: {CommandName} | Outcome: success | Duration: {Duration}s",
                commandName, elapsedSeconds);
                return response;
            }
            catch (Exception ex)
            {
                CommandMetrics.Failures.Add(1,
                    new TagList
                    {
                        { "command", commandName },
                        { "failure_type", Classify(ex) }
                    });

                RecordDuration(commandName, "failure", startTimestamp);
                throw;
            }
        }

        private static void RecordDuration(
            string commandName,
            string outcome,
            long startTimestamp)
        {
            var elapsedSeconds =
                Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds;

            CommandMetrics.Duration.Record(
                elapsedSeconds,
                new TagList
                {
                    { "command", commandName },
                    { "outcome", outcome }
                });
        }

        private static string Classify(Exception ex)
        {
            return ex switch
            {
                ValidationException => "business_validation",
               // FlameApplicationException => "business_exception",
                _ => "technical"
            };
        }
    }
}
