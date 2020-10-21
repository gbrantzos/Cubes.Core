using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Metrics;
using Cubes.Core.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Commands
{
    public class RequestLoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<RequestLoggingBehaviour<TRequest, TResponse>> _logger;
        private readonly IMetrics _metrics;

        public RequestLoggingBehaviour(ILogger<RequestLoggingBehaviour<TRequest, TResponse>> logger, IMetrics metrics)
        {
            _logger = logger;
            _metrics = metrics;
        }

        public async Task<TResponse> Handle(TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            _logger.LogDebug("Executing => [{requestType}] {request}", typeof(TRequest).Name, request);
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                var result = await next();
                sw.Stop();

                _metrics
                    .GetCounter(CubesCoreMetrics.CubesRequests)
                    .WithLabels(typeof(TRequest).Name)
                    .Inc();

                if (result is IResult requestResult)
                {
                    var formattedMessage = AddIndent(requestResult.Message);
                    if (requestResult.HasErrors)
                    {
                        // TODO This can be controlled by options
                        if (requestResult.ExceptionThrown != null)
                        {
                            _logger.LogError(requestResult.ExceptionThrown,
                                $"Result has errors => [{typeof(TRequest).Name}] {request} ({sw.ElapsedMilliseconds}ms)\r\n{formattedMessage}");
                            // TODO This can be controlled by options
                            //_logger.LogError("{@request}", request);
                        }
                        else
                            _logger.LogError($"Result has errors => [{typeof(TRequest).Name}] {request} ({sw.ElapsedMilliseconds}ms)\r\n{formattedMessage}");
                    }
                    else
                    {
                        var msg = requestResult.Message == requestResult.DefaultMessage ?
                            String.Empty :
                            $"\r\n{formattedMessage}";
                        _logger.LogInformation($"Executed => [{typeof(TRequest).Name}] {request} ({sw.ElapsedMilliseconds}ms){msg}");
                    }
                }
                else
                {
                    // No more details to display
                    _logger.LogInformation($"Request executed, {typeof(TRequest).Name}: {request} [{sw.ElapsedMilliseconds}ms]");
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
    }
}
