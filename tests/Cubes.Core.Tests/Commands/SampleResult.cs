using Cubes.Core.Commands;

namespace Cubes.Core.Tests.Commands
{
    public class SampleResult : ICommandResult
    {
        public bool HasErrors { get; set; }
        public string Message { get; set; }
    }
}