using System;
using System.Linq;

namespace Cubes.Core.Commands
{
    public class CommandBus : ICommandBus
    {
        private readonly ServiceFactory handlerFactory;

        public CommandBus(ServiceFactory handlerFactory)
            => this.handlerFactory = handlerFactory;

        public TResult Submit<TResult>(ICommand<TResult> command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var handler = (CommandHandlerHelperBase<TResult>)Activator
                .CreateInstance(typeof(CommandHandlerHelper<,>).MakeGenericType(command.GetType(), typeof(TResult)), handlerFactory);

            return handler.Handle(command);
        }
    }

    #region Helper classes
    internal abstract class CommandHandlerHelperBase<TResult>
    {
        protected readonly ServiceFactory serviceFactory;

        public CommandHandlerHelperBase(ServiceFactory serviceFactory)
            => this.serviceFactory = serviceFactory;

        public abstract TResult Handle(ICommand<TResult> command);
    }

    internal sealed class CommandHandlerHelper<TCommand, TResult> : CommandHandlerHelperBase<TResult>
        where TCommand : ICommand<TResult>
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