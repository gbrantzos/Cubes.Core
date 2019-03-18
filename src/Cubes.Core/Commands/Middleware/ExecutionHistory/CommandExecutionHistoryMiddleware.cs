using System;
using System.Diagnostics;
using Cubes.Core.Settings;

namespace Cubes.Core.Commands.Middleware.ExecutionHistory
{
    public class CommandExecutionHistoryMiddleware<TCommand, TResult> : ICommandBusMiddleware<TCommand, TResult> where TResult : ICommandResult
    {
        private readonly ISettingsProvider settingsProvider;
        private readonly ICommandExecutionHistory executionHistory;

        public CommandExecutionHistoryMiddleware(ISettingsProvider settingsProvider, ICommandExecutionHistory executionHistory)
        {
            this.settingsProvider = settingsProvider;
            this.executionHistory = executionHistory;
        }

        public TResult Execute(TCommand command, CommandHandlerDelegate<TResult> next)
        {
            var settings = settingsProvider.Load<CommandExecutionHistorySettings>();
            var commandType = typeof(TCommand).FullName;

            var execution = new CommandExecution
            {
                CommandType = commandType,
                ExecutedAt  = DateTime.Now
            };

            var sw = new Stopwatch();
            sw.Start();

            var res = next();
            sw.Stop();

            if (!settings.SkipCommands.Contains(commandType))
            {
                execution.Duration = sw.Elapsed;
                execution.Result   = res.ExecutionResult;
                execution.Message  = res.Message;

                executionHistory.Add(execution);
            }
            return res;
        }
    }
}
