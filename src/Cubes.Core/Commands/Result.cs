using System;

namespace Cubes.Core.Commands
{
    public class Result<TResponse> : IResult
    {
        /// <summary>
        /// True if execution failed
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        /// Message for caller
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The actual response of type TResponse
        /// </summary>
        public TResponse Response { get; set; }

        /// <summary>
        /// Any <see cref="Exception"/> thrown during execution and not handled by
        /// <see cref="RequestHandler{TRequest, TResponse}.Handle(TRequest, System.Threading.CancellationToken)"/>
        /// </summary>
        public Exception ExceptionThrown { get; set; }

        /// <summary>
        /// The actual response of type TResponse
        /// To be used when Response type is not known
        /// </summary>
        object IResult.Response { get => Response; }
    }

    // Helper interface
    public interface IResult
    {
        /// <summary>
        /// True if execution failed
        /// </summary>
        bool HasErrors            { get; }

        /// <summary>
        /// Message for caller
        /// </summary>
        string Message            { get; }

        /// <summary>
        /// The actual response instance. To be used when Response type is not known
        /// </summary>
        object Response           { get; }

        /// <summary>
        /// Any <see cref="Exception"/> thrown during execution and not handled by request handler
        /// </summary>
        Exception ExceptionThrown { get; }
    }
}
