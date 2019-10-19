using System;

namespace Cubes.Core.Commands
{
    public class Result<TResponse>
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
        /// The actual response of type <see cref="TResponse"/>
        /// </summary>
        public TResponse Response { get; set; }

        /// <summary>
        /// Any <see cref="Exception"/> thrown during execution and not handled by
        /// <see cref="RequestHandler{TRequest, TResponse}.Handle(TRequest, System.Threading.CancellationToken)"/>
        /// </summary>
        public Exception ExceptionThrown { get; set; }
    }
}
