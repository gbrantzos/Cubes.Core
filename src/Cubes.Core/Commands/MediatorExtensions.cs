using System;
using System.Linq;
using System.Threading;
using MediatR;

namespace Cubes.Core.Commands
{
    public static class MediatorExtensions
    {
        /// <summary>
        /// Send a request without knowing the result type.
        /// <para>
        /// Although this method returns dynamic we can safely assume it will return an await-able
        /// with <see cref="IResult"/> instance as result.
        /// </para>
        /// </summary>
        /// <param name="mediator">Command bus</param>
        /// <param name="request">Command to execute</param>
        /// <returns>Result as ICommandResult</returns>
        public static dynamic Send(this IMediator mediator, object request)
        {
            // Request type
            var requestType = request.GetType();

            // Find commands result through reflection
            var resultType = requestType.GetRequestType();
            if (resultType == null)
                throw new ArgumentException($"No request type found for type {requestType.Name}");

            // Create generic method for given result type and invoke
            var submitMethod = typeof(Mediator)
                .GetMethod(nameof(Mediator.Send))
                .MakeGenericMethod(resultType);
            return (dynamic)submitMethod.Invoke(mediator, new object[] { request , CancellationToken.None });
        }

        /// <summary>
        /// Helper method to determine if a given type is a MediatR <see cref="IRequest"/>.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns></returns>
        public static bool IsMediatorRequest(this Type type)
            => type.GetInterfaces()
                .Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition().Equals(typeof(IRequest<>))
                )
                && type.IsClass
                && !type.IsAbstract;

        /// <summary>
        /// Get corresponding response type from given request type
        /// </summary>
        /// <param name="requestType">The command type</param>
        /// <returns></returns>
        private static Type GetRequestType(this Type requestType)
            => Array
                .Find(requestType.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))?
                .GetGenericArguments()?
                .FirstOrDefault();
    }
}
