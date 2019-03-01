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
                // Gather messages
                var messages = ex.GetInnerExceptions()
                    .Select(x => $"{x.GetType().Name}: {x.Message}")
                    .Aggregate((res, val) => $"{res}{System.Environment.NewLine}{val}");

                // Handle exception
                throw new CommandExecutionException<TCommand>("Command execution failed!", ex)
                {
                    Command            = command,
                    CommandHandlerType = this.GetType(),
                    ErrorMessage       = messages
                };
            }
        }

        protected abstract TResult HandleInternal(TCommand command);
    }
}