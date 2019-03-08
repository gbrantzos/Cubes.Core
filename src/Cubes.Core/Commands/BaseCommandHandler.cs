using System;
using System.Linq;
using Cubes.Core.Utilities;

namespace Cubes.Core.Commands
{
    public abstract class BaseCommandHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
        where TResult : ICommandResult
    {
        public TResult Handle(TCommand command)
        {
            try
            {
                return HandleInternal(command);
            }
            catch (Exception ex)
            {
                // CommandExecutionException should signal a LOGICAL error
                if (ex.GetType().IsSubclassOf(typeof(CommandExecutionException<>)))
                    throw;

                // Handle exception
                throw new CommandExecutionException<TCommand>("Command execution failed!", ex)
                {
                    Command            = command,
                    CommandHandlerType = this.GetType(),
                    ErrorMessage       = ex.Message
                };
            }
        }

        protected abstract TResult HandleInternal(TCommand command);
    }
}