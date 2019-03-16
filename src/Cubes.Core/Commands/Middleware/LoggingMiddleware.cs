using System;
using System.Collections.Generic;
using System.Text;
using Cubes.Core.Settings;

namespace Cubes.Core.Commands.Middleware
{
    public class LoggingMiddleware<TCommand, TResult> : ICommandBusMiddleware<TCommand, TResult> where TResult : ICommandResult
    {
        private readonly ISettingsProvider settings;

        public LoggingMiddleware(ISettingsProvider settings)
        {
            this.settings = settings;
        }

        public TResult Execute(TCommand command, CommandHandlerDelegate<TResult> next)
        {
            Console.WriteLine("Before execution");
            var res = next();
            Console.WriteLine("After execution");

            return res;
        }
    }
}
