using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Utilities;
using MediatR;

namespace Cubes.Core.Commands
{
    public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse>>
        where TRequest : IRequest<Result<TResponse>>
    {
        /// <summary>
        /// Set by the internal Handler if we need to define message for <see cref="Result{TResponse}"/>
        /// </summary>
        protected string MessageToReturn { get; set; }

        /// <summary>
        /// The actual handler with specific business logic
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<TResponse> HandleInternal(TRequest request, CancellationToken cancellationToken);

        public async Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var toReturn = new Result<TResponse>();
            try
            {
                toReturn.Response = await this.HandleInternal(request, cancellationToken);
                toReturn.Message = MessageToReturn.IfNullOrEmpty($"{typeof(TRequest).Name} was executed successfully!");
            }
            catch (Exception x)
            {
                toReturn.HasErrors = true;
                toReturn.ExceptionThrown = x;

                var allMesages = x
                    .FromHierarchy(x => x.InnerException)
                    .Select(x => x.Message)
                    .Distinct()
                    .ToList();
                toReturn.Message = MessageToReturn.IfNullOrEmpty(String.Join(System.Environment.NewLine, allMesages));
            }
            return toReturn;
        }
    }
}
