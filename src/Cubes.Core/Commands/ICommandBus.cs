using System;
using System.Linq;

namespace Cubes.Core.Commands
{
    /// <summary>
    /// Command bus
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Submit a Command for execution
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <returns></returns>
        TResult Submit<TResult>(ICommand<TResult> command) where TResult : ICommandResult, new();
    }

    public static class CommandBusExtensions
    {
        public static ICommandResult Submit(this ICommandBus bus, object command)
        {
            // Command type
            var commandType = command.GetType();

            // Find commands result through reflection
            var resultType = commandType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
                .FirstOrDefault()?
                .GetGenericArguments()?
                .FirstOrDefault();
            if (resultType == null)
                throw new ArgumentException($"No CommandResult type found for type {commandType.Name}");

            // Create generic method for given result type and invoke
            var submitMethod = typeof(CommandBus)
                .GetMethod("Submit")
                .MakeGenericMethod(resultType);
            return submitMethod.Invoke(bus, new object[] { command }) as ICommandResult;
        }
    }
}