using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Cubes.Core.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Get first Attribute of type TAttribute for given type
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type</typeparam>
        /// <param name="type">Requested type</param>
        /// <param name="throwOnError">Throw exception if not found</param>
        /// <returns></returns>
        public static TAttribute GetAttribute<TAttribute>(this Type type, bool throwOnError) where TAttribute : Attribute
        {
            var attr = type
                .GetCustomAttributes(true)
                .Where(i => i.GetType().Equals(typeof(TAttribute)))
                .FirstOrDefault();
            if ((attr == null || !(attr is TAttribute)) && throwOnError)
                throw new Exception($"Could not find attribute '{ typeof(TAttribute).Name }' for type { type.Name }");

            return attr as TAttribute;
        }
        public static TAttribute GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
            => type.GetAttribute<TAttribute>(false);

        /// <summary>
        /// Returns property values of an object as a dictionary with keys the property name
        /// </summary>
        /// <param name="obj">Object to analyze</param>
        /// <returns></returns>
        public static Dictionary<string, object> AsPropertiesDictionary(this object obj)
        {
            var returnValue = new Dictionary<string, object>();

            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
                returnValue.Add(prop.Name, prop.GetValue(obj));

            return returnValue;
        }

        /// <summary>
        /// If <see cref="String"/> s is null or empty return retValue.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="retValue"></param>
        /// <returns></returns>
        public static string IfNullOrEmpty(this string s, string retValue) => String.IsNullOrEmpty(s) ? retValue : s;

        /// <summary>
        /// Get a list of all inner exceptions
        /// </summary>
        /// <param name="ex">Original exception</param>
        /// <returns>IEnumerable&lt;Exception&gt;</returns>
        public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
        {
            var toReturn = new List<Exception>();
            var current = ex;

            while (current != null)
            {
                if (!toReturn.Contains(current))
                    toReturn.Add(current);
                current = current.InnerException;
            }
            return toReturn;
        }

        /// <summary>
        /// Get property of object with type TProperty. Object must be nullable.
        /// </summary>
        /// <typeparam name="TProperty">Type of property to find</typeparam>
        /// <param name="obj">Object to search for property</param>
        /// <returns></returns>
        public static TProperty GetPropertyByType<TProperty>(this object obj) where TProperty : class
        {
            if (obj == null) return null;
            if (obj is TProperty)
                return (TProperty)obj;

            var props = obj.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).ToArray();
            if (props.Length == 0) return null;

            foreach (var prop in props)
            {
                var propType = prop.PropertyType;
                var propValue = prop.GetValue(obj);

                if (propValue == null) continue;
                if (propType.Equals(typeof(TProperty)))
                    return (TProperty)propValue;

                var stopRecursion = propType.IsPrimitive ||
                    propType.Equals(typeof(string)) ||
                    propType.Equals(typeof(DataTable)) ||
                    (propType.IsGenericType && propType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>))) ||
                    (propType.IsGenericType && propType.GetGenericTypeDefinition().IsAssignableFrom(typeof(IEnumerable<>)));

                if (!stopRecursion && propValue != null)
                {
                    var retValue = propValue.GetPropertyByType<TProperty>();
                    if (retValue != null) return retValue;
                }
            }

            return null;
        }
    }
}