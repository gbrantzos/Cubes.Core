using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Metrics;
using Cubes.Core.Utilities;
using DotNet.Globbing;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cubes.Core.Commands
{
    public class RequestLoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<RequestLoggingBehaviour<TRequest, TResponse>> _logger;
        private readonly IMetrics _metrics;
        private readonly RequestLoggingOptions _options;

        public RequestLoggingBehaviour(ILogger<RequestLoggingBehaviour<TRequest, TResponse>> logger,
            IMetrics metrics,
            IOptionsSnapshot<RequestLoggingOptions> optionsSnapshot)
        {
            _logger = logger;
            _metrics = metrics;
            _options = optionsSnapshot.Value;
        }

        public async Task<TResponse> Handle(TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var requestName = typeof(TRequest).Name;
            var requestType = typeof(TRequest).FullName;
            _logger.LogDebug("[{requestName:l}] Executing => {request}", requestName, request);
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                var result = await next();
                sw.Stop();

                _metrics
                    .GetCounter(CubesCoreMetrics.CubesCoreRequestsCount)
                    .WithLabels(requestType)
                    .Inc();
                _metrics
                    .GetHistogram(CubesCoreMetrics.CubesCoreRequestsDuration)
                    .WithLabels(requestType)
                    .Observe(sw.Elapsed.TotalSeconds);

                if (result is Result requestResult)
                {
                    var formattedMessage = AddIndent(requestResult.Message);
                    if (requestResult.HasErrors)
                    {
                        _logger.LogError($"[{requestName}] Request result has errors => {request} ({sw.ElapsedMilliseconds}ms)\r\n{formattedMessage}");

                        if (requestResult.ExceptionThrown != null && ShouldLogException(requestType))
                            _logger.LogError(requestResult.ExceptionThrown, $"[{requestName}] Exception thrown =>");
                        if (ShouldLogRequest(requestType))
                            _logger.LogError("[{requestName:l}] Request details =>\r\nRequest type :: {requestType:l}\r\n{@request}",
                                requestName,
                                requestType,
                                request);
                    }
                    else
                    {
                        var msg = requestResult.Message == requestResult.DefaultMessage() ?
                            String.Empty :
                            $"\r\n{formattedMessage}";
                        _logger.LogInformation($"[{requestName}] Executed successfully => {request} ({sw.ElapsedMilliseconds}ms){msg}");
                    }
                }
                else
                {
                    // No details to display
                    _logger.LogInformation($"Request executed, {requestName}: {request} [{sw.ElapsedMilliseconds}ms]");
                }
                return result;
            }
            catch (Exception x)
            {
                var all     = x.GetAllMessages();
                var message = String.Join(Environment.NewLine, all);
                _logger.LogError(x, "Request execution failed: {message}\r\n{@request}", message, request);

                throw;
            }
        }

        private static string AddIndent(string message)
        {
            if (String.IsNullOrEmpty(message))
                return message;

            var lines = message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (var line in lines)
                sb.AppendLine($"    {line}");

            return sb.ToString().Trim(new char[] { '\r', '\n' });
        }

        private bool ShouldLogException(string requestType)
        {
            foreach (var item in _options.LogExceptionsForRequests)
            {
                var glob = Glob.Parse(item);
                if (glob.IsMatch(requestType))
                    return true;
            }
            return false;
        }

        private bool ShouldLogRequest(string requestType)
        {
            foreach (var item in _options.LogFailedRequests)
            {
                var glob = Glob.Parse(item);
                if (glob.IsMatch(requestType))
                    return true;
            }
            return false;
        }
    }
}
