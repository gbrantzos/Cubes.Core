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
        /// <summary>
        /// submit a command without knowing the result type
        /// </summary>
        /// <param name="bus">Command bus</param>
        /// <param name="command">Command to execute</param>
        /// <returns>Result as ICommandResult</returns>
        public static ICommandResult Submit(this ICommandBus bus, object command)
        {
            // Command type
            var commandType = command.GetType();

            // Find commands result through reflection
            var resultType = commandType.GetResultType();
            if (resultType == null)
                throw new ArgumentException($"No CommandResult type found for type {commandType.Name}");

            // Create generic method for given result type and invoke
            var submitMethod = typeof(CommandBus)
                .GetMethod("Submit")
                .MakeGenericMethod(resultType);
            return submitMethod.Invoke(bus, new object[] { command }) as ICommandResult;
        }

        /// <summary>
        /// Helper method to  determine if a given type is ICommand
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns></returns>
        public static bool IsCommand(this Type type)
            => type.GetInterfaces().Any(i => i.IsGenericType
            && i.GetGenericTypeDefinition().Equals(typeof(ICommand<>)))
            && type.IsClass && !type.IsAbstract;

        /// <summary>
        /// Get corresponding result type from given command type
        /// </summary>
        /// <param name="commandType">The command type</param>
        /// <returns></returns>
        public static Type GetResultType(this Type commandType)
            => commandType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
                .FirstOrDefault()?
                .GetGenericArguments()?
                .FirstOrDefault();
    }
}