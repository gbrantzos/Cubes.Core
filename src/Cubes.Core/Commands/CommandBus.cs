using System;
using System.Linq;
using Cubes.Core.Utilities;

namespace Cubes.Core.Commands
{
    public class CommandBus : ICommandBus
    {
        private readonly ServiceFactory handlerFactory;

        public CommandBus(ServiceFactory handlerFactory)
            => this.handlerFactory = handlerFactory;

        public TResult Submit<TResult>(ICommand<TResult> command) where TResult : ICommandResult, new()
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var handler = (CommandHandlerHelperBase<TResult>)Activator
                .CreateInstance(typeof(CommandHandlerHelper<,>).MakeGenericType(command.GetType(), typeof(TResult)), handlerFactory);

            try
            {
                return handler.Handle(command);
            }
            catch (Exception ex)
            {
                // If no handler found just throw (it's definetelly internal error)
                if (ex is CommandHandlerResolveException)
                    throw;

                var toReturn = new TResult();

                // Try to be as nice as possible to caller
                if (ex.GetType().IsSubclassOf(typeof(CommandExecutionException<>)))
                {
                    // It would be nice for handlers to throw CommandExecutionException to signal a LOGICAL error
                    toReturn.ExecutionResult = CommandExecutionResult.Error;
                    toReturn.Message = ex.Message;
                }
                else
                {
                    // Gather messages
                    var messages = ex.GetInnerExceptions()
                        .Select(x => $"{x.GetType().Name}: {x.Message}")
                        .Aggregate((res, val) => $"{res}{System.Environment.NewLine}{val}");

                    toReturn.ExecutionResult = CommandExecutionResult.Failure;
                    toReturn.Message = messages;
                }

                return toReturn;
            }
        }
    }

    #region Helper classes
    internal abstract class CommandHandlerHelperBase<TResult> where TResult : ICommandResult
    {
        protected readonly ServiceFactory serviceFactory;

        public CommandHandlerHelperBase(ServiceFactory serviceFactory)
            => this.serviceFactory = serviceFactory;

        public abstract TResult Handle(ICommand<TResult> command);
    }

    internal sealed class CommandHandlerHelper<TCommand, TResult> : CommandHandlerHelperBase<TResult>
        where TCommand : ICommand<TResult>
        where TResult : ICommandResult
    {
        public CommandHandlerHelper(ServiceFactory handlerFactory) : base(handlerFactory) { }

        public override TResult Handle(ICommand<TResult> command)
        {
            // Resolve handler
            TCommandHandler ResolveHandler<TCommandHandler>()
            {
                TCommandHandler toReturn;
                try
                { toReturn = serviceFactory.GetInstance<TCommandHandler>(); }
                catch (Exception x)
                { throw new CommandHandlerResolveException(typeof(TCommand), "Failed to resolve CommandHandler!", x); }

                if (toReturn == null)
                    throw new CommandHandlerResolveException(typeof(TCommand), "Failed to resolve CommandHandler!");
                return toReturn;
            }

            TResult baseHandler()
                => ResolveHandler<ICommandHandler<TCommand, TResult>>().Handle((TCommand)command);

            var middlewareHandlers = serviceFactory.GetInstances<ICommandBusMiddleware<TCommand, TResult>>();
            var combinedHandler = middlewareHandlers
                .Reverse()
                .Aggregate((CommandHandlerDelegate<TResult>)baseHandler, (next, middleware) => () =>
                    {
                        // Do something Before, logging, timing...
                        return middleware.Execute((TCommand)command, next);
                        // Do something After
                    });
            return combinedHandler();
        }
    }
    #endregion
}