using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Commands
{
    public class DefaultBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<DefaultBehavior<TRequest, TResponse>> logger;

        public DefaultBehavior(ILogger<DefaultBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            logger.LogDebug("Executing => [{requestType}] {request}", typeof(TRequest).Name, request);
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                var result = await next();
                sw.Stop();

                if (result is IResult requestResult)
                {
                    if (requestResult.HasErrors)
                    {
                        if (requestResult.ExceptionThrown != null)
                        {
                            logger.LogWarning(requestResult.ExceptionThrown,
                                $"Result has errors => [{typeof(TRequest).Name}] {request} ({sw.ElapsedMilliseconds}ms): {requestResult.Message}\r\nException thrown:");
                        }
                        else
                            logger.LogWarning($"Result has errors => [{typeof(TRequest).Name}] {request} ({sw.ElapsedMilliseconds}ms): {requestResult.Message}");
                    }
                    else
                    {
                        var msg = requestResult.Message == requestResult.DefaultMessage ?
                            String.Empty :
                            $", message:\r\n{requestResult.Message}";
                        logger.LogInformation($"Executed => [{typeof(TRequest).Name}] {request} in {sw.ElapsedMilliseconds}ms{msg}");
                    }
                }
                else
                {
                    // No more details to display
                    logger.LogInformation($"Request executed, {typeof(TRequest).Name}: {request} [{sw.ElapsedMilliseconds}ms]");
                }
                return result;
            }
            catch (Exception x)
            {
                var all     = x.GetAllMessages();
                var message = String.Join(Environment.NewLine, all);
                logger.LogError(x, "Request execution failed: {message}\r\n{@request}", message, request);

                throw;
            }
        }
    }
}
