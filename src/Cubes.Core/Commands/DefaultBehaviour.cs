using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Commands
{
    public class DefaultBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<DefaultBehaviour<TRequest, TResponse>> logger;

        public DefaultBehaviour(ILogger<DefaultBehaviour<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            logger.LogInformation("Executing request {requestType}: {request}", typeof(TRequest).Name, request);
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                var result = await next();
                sw.Stop();

                logger.LogInformation("Request executed, elapsed {elapsedTime}ms", sw.ElapsedMilliseconds);
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
