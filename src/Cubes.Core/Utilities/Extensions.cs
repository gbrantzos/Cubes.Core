using System;
using System.Collections.Generic;
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
    }
}