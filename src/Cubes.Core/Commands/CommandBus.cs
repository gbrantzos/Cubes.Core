using System;

namespace Cubes.Core.Commands
{
    public class CommandBus : ICommandBus
    {
        private readonly CommandHandlerFactory handlerFactory;

        public CommandBus(CommandHandlerFactory handlerFactory)
            => this.handlerFactory = handlerFactory;

        public TResult Submit<TResult>(ICommand<TResult> command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var handler = (CommandHandlerHelperBase<TResult>)Activator
                .CreateInstance(typeof(CommandHandlerHelper<,>).MakeGenericType(command.GetType(), typeof(TResult)), handlerFactory);

            return handler.Handle(command);
        }


        internal abstract class CommandHandlerHelperBase<TResult>
        {
            protected readonly CommandHandlerFactory handlerFactory;

            public CommandHandlerHelperBase(CommandHandlerFactory handlerFactory)
                => this.handlerFactory = handlerFactory;

            public abstract TResult Handle(ICommand<TResult> command);
        }

        internal sealed class CommandHandlerHelper<TCommand, TResult> : CommandHandlerHelperBase<TResult>
            where TCommand : ICommand<TResult>
        {
            public CommandHandlerHelper(CommandHandlerFactory handlerFactory) : base(handlerFactory) { }

            public override TResult Handle(ICommand<TResult> command)
            {
                ICommandHandler<TCommand, TResult> handler = null;
                try
                {
                    handler = handlerFactory.GetInstance<ICommandHandler<TCommand, TResult>>();
                }
                catch (Exception x)
                {
                    throw new CommandHandlerResolveException(typeof(TCommand), "Failed to resolve CommandHandler!", x);
                }

                // We can safely cast command to TCommand, thanks to generic constraint
                return handler.Handle((TCommand)command);
            }
        }
    }
}