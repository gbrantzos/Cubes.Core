using System;

namespace Cubes.Core.Commands
{
    public class CommandExecutionException<TCommand> : Exception
    {
        public TCommand Command { get; set; }
        public Type CommandHandlerType { get; set; }
        public string ErrorMessage { get; set; }

        public CommandExecutionException(string message, Exception ex) : base(message, ex) { }
    }

}
