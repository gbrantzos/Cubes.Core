using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cubes.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Commands
{
    public class CommandBus : ICommandBus
    {
        private readonly ServiceFactory handlerFactory;    
        private readonly ILogger<CommandBus> logger;

        public CommandBus(ServiceFactory handlerFactory, ILoggerFactory loggerFactory)
        {
            this.handlerFactory = handlerFactory;
            this.logger = loggerFactory.CreateLogger<CommandBus>();
        }

        public TResult Submit<TResult>(ICommand<TResult> command) where TResult : ICommandResult, new()
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var handler = (CommandHandlerHelperBase<TResult>)Activator
                .CreateInstance(typeof(CommandHandlerHelper<,>).MakeGenericType(command.GetType(), typeof(TResult)), handlerFactory, logger);

            try
            {
                logger.LogInformation($"Command submitted => {command.ToString()}");
                var result = handler.Handle(command);
                logger.LogInformation($"Command result => {result.Message}");

                return result;
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

                logger.LogWarning($"Command result => {toReturn.Message}");
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
        private readonly ILogger logger;

        public CommandHandlerHelper(ServiceFactory handlerFactory, ILogger logger) : base(handlerFactory)
            => this.logger = logger;

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

            var tracking = new List<TrackInfo>();
            var middlewareHandlers = serviceFactory.GetInstances<ICommandBusMiddleware<TCommand, TResult>>();
            var combinedHandler = middlewareHandlers
                .Reverse()
                .Aggregate((CommandHandlerDelegate<TResult>)baseHandler, (next, middleware) => () =>
                    {
                        var middlewareType = middleware.GetType();
                        var ti = new TrackInfo
                        {
                            MiddlewareType = middlewareType.IsGenericType ?
                                middlewareType
                                    .GetGenericTypeDefinition()
                                    .FullName
                                    .RemoveSuffix("`2") :
                                middlewareType.GetType().FullName,
                            StartedAt      = DateTime.Now
                        };
                        tracking.Add(ti);

                        var sw = new Stopwatch();
                        sw.Start();

                        var result = middleware.Execute((TCommand)command, next);

                        sw.Stop();
                        ti.Duration = sw.Elapsed;

                        return result;
                    });

            var commandResult = combinedHandler();

            logger.LogDebug($"CommandBus middleware types found: {tracking.Count}");
            foreach (var item in tracking)
                logger.LogDebug("At {startedAt} {middlewaretype} ({duration}ms)",
                    item.StartedAt.ToString("HH:mm:ss.fff"),
                    item.MiddlewareType,
                    item.Duration);

            return commandResult;
        }
    }

    internal sealed class TrackInfo
    {
        public string MiddlewareType { get; set; }
        public DateTime StartedAt { get; set; }
        public TimeSpan Duration { get; set; }

    }
    #endregion
}