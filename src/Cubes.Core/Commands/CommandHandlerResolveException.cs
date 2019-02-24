using System;

namespace Cubes.Core.Commands
{
    public class CommandHandlerResolveException : Exception
    {
        public Type CommandType { get; protected set; }

        public CommandHandlerResolveException(Type commandType, string message, Exception innerException) : base(message, innerException) 
            => CommandType = commandType;

        public CommandHandlerResolveException(Type commandType, string message) : this(commandType, message, null) { }
    }
}