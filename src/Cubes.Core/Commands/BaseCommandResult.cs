namespace Cubes.Core.Commands
{
    public abstract class BaseCommandResult : ICommandResult
    {
        public virtual bool HasErrors { get; set; }
        public virtual string Message { get; set; }
    }
}
