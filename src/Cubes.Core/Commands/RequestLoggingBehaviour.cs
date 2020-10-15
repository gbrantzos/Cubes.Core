using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Commands
{
    public class RequestLoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<RequestLoggingBehaviour<TRequest, TResponse>> _logger;

        public RequestLoggingBehaviour(ILogger<RequestLoggingBehaviour<TRequest, TResponse>> logger)
            => _logger = logger;

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

                if (result is IResult requestResult)
                {
                    var formattedMessage = AddIndent(requestResult.Message);
                    if (requestResult.HasErrors)
                    {
                        if (requestResult.ExceptionThrown != null)
                        {
                            _logger.LogWarning(requestResult.ExceptionThrown,
                                $"Result has errors => [{typeof(TRequest).Name}] {request} ({sw.ElapsedMilliseconds}ms): {requestResult.Message}\r\nException thrown:");
                        }
                        else
                            _logger.LogWarning($"Result has errors => [{typeof(TRequest).Name}] {request} ({sw.ElapsedMilliseconds}ms): {formattedMessage}");
                    }
                    else
                    {
                        var msg = requestResult.Message == requestResult.DefaultMessage ?
                            String.Empty :
                            $", message:\r\n{formattedMessage}";
                        _logger.LogInformation($"Executed => [{typeof(TRequest).Name}] {request} in {sw.ElapsedMilliseconds}ms{msg}");
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
