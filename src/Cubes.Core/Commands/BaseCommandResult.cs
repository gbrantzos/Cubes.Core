namespace Cubes.Core.Commands
{
    public abstract class BaseCommandResult : ICommandResult
    {
        public virtual bool HasErrors { get; set; }
        public virtual string Message { get; set; }

        public override string ToString()
        {
            var toReturn = HasErrors ?
                "Execution failed" :
                "Execution was successful";
            if (!string.IsNullOrEmpty(Message))
                toReturn += $", {Message}";
            toReturn += ".";

            return toReturn;
        }
    }
}
