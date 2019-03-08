using System;
using System.Collections.Generic;
using System.Text;

namespace Cubes.Core.Commands
{
    // Command execution result enum
    public enum CommandExecutionResult
    {
        /// <summary>
        /// Command result was successfull
        /// </summary>
        Success,

        /// <summary>
        /// Command failed due to business - logical errors
        /// </summary>
        Error,

        /// <summary>
        /// Command failed due to unhandled - system errors
        /// </summary>
        Failure
    }

    /// <summary>
    /// Command result
    /// </summary>
    public interface ICommandResult
    {
        /// <summary>
        /// Execution result enum. Should separate logical errors from failures
        /// </summary>
        /// <value></value>
        CommandExecutionResult ExecutionResult { get; set; }

        /// <summary>
        /// Result message or messages for feedback
        /// </summary>
        /// <value></value>
        string Message { get; set; }
    }
}
