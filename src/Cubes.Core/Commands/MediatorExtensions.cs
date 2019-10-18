using System;
using System.Linq;
using System.Threading;
using MediatR;

namespace Cubes.Core.Commands
{
    public static class MediatorExtensions
    {
        /// <summary>
        /// submit a command without knowing the result type
        /// </summary>
        /// <param name="mediator">Command bus</param>
        /// <param name="command">Command to execute</param>
        /// <returns>Result as ICommandResult</returns>
        public static dynamic Send(this IMediator mediator, object command)
        {
            // Command type
            var commandType = command.GetType();

            // Find commands result through reflection
            var resultType = commandType.GetRequestType();
            if (resultType == null)
                throw new ArgumentException($"No request type found for type {commandType.Name}");

            // Create generic method for given result type and invoke
            var submitMethod = typeof(Mediator)
                .GetMethod(nameof(Mediator.Send))
                .MakeGenericMethod(resultType);
            return (dynamic)submitMethod.Invoke(mediator, new object[] { command , CancellationToken.None });
        }

        /// <summary>
        /// Helper method to  determine if a given type is ICommand
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns></returns>
        public static bool IsMediatorRequest(this Type type)
            => type.GetInterfaces().Any(i => i.IsGenericType
            && i.GetGenericTypeDefinition().Equals(typeof(IRequest<>)))
            && type.IsClass && !type.IsAbstract;

        /// <summary>
        /// Get corresponding response type from given request type
        /// </summary>
        /// <param name="requestType">The command type</param>
        /// <returns></returns>
        private static Type GetRequestType(this Type requestType)
            => requestType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                .FirstOrDefault()?
                .GetGenericArguments()?
                .FirstOrDefault();
    }
}
