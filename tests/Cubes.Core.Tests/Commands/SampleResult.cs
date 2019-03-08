using Cubes.Core.Commands;

namespace Cubes.Core.Tests.Commands
{
    public class SampleResult : ICommandResult
    {
        public CommandExecutionResult ExecutionResult{ get; set; }
        public string Message { get; set; }
    }
}