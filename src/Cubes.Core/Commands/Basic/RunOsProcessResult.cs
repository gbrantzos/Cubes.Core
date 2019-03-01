namespace Cubes.Core.Commands.Basic
{
    public class RunOsProcessResult : BaseCommandResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; }
    }
}