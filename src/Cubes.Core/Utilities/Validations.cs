using System;

namespace Cubes.Core.Utilities
{
    public static class CubesValidations
    {
        /// <summary>
        /// Handling the null check and throw a ArgumentNullException if null
        /// </summary>
        /// <typeparam name="T">The type of the parameter</typeparam>
        /// <param name="parameter">the actual parameter</param>
        /// <param name="parameterName">the name of the parameter</param>
        /// <param name="message">an optional message instead of the default</param>
        public static T ThrowIfNull<T>(this T parameter, string parameterName, string message = null) where T : class
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(parameterName, message ?? $"Parameter '{parameterName}' is null!");
            }
            return parameter;
        }

        /// <summary>
        /// Handling string empty check and throw a ArgumentNullException if null
        /// </summary>
        /// <param name="parameter">the actual string parameter</param>
        /// <param name="parameterName">the name of the parameter</param>
        /// <param name="message">an optional message instead of the default</param>
        public static string ThrowIfEmpty(this string parameter, string parameterName, string message = null)
        {
            if (String.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException(parameterName, message ?? $"Parameter '{parameterName}' is null or empty!");
            }
            return parameter;
        }
    }
}