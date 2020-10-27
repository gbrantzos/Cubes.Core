using System;

namespace Cubes.Core.Commands
{
    public class Result
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
        /// Any <see cref="Exception"/> thrown during execution and not handled by
        /// <see cref="RequestHandler{TRequest, TResponse}.Handle(TRequest, System.Threading.CancellationToken)"/>
        /// </summary>
        public Exception ExceptionThrown { get; set; }
    }

    public class Result<TResponse> : Result
    {
        /// <summary>
        /// The actual response of type TResponse
        /// </summary>
        public TResponse Response { get; set; }
    }

    public static class ResultExtensions
    {
        public static string DefaultMessage(this Result result)
            => $"{result.GetType().Name} was executed successfully!";
    }
}
