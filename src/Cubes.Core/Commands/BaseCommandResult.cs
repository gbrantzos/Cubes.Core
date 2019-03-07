using System;

namespace Cubes.Core.Commands
{
    public abstract class BaseCommandResult : ICommandResult
    {
        public virtual CommandExecutionResult ExecutionResult { get; set; } = CommandExecutionResult.Success;
        public virtual string Message { get; set; }

        public override string ToString()
        {
            var toReturn = String.Empty;
            switch (ExecutionResult)
            {
                case CommandExecutionResult.Success:
                    toReturn = "Execution was successful.";
                    break;
                case CommandExecutionResult.Error:
                    toReturn = "Execution finished with errors.";
                    break;
                case CommandExecutionResult.Failure:
                    toReturn = "Execution FAILED!";
                    break;
            }
            if (!string.IsNullOrEmpty(Message))
                toReturn += $" {Message}";
            return toReturn;
        }
    }
}
