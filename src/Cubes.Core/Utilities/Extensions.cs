using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Cubes.Core.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Determine whether the object is real - non-abstract, non-generic-needed, non-interface class.
        /// </summary>
        /// <param name="testType">Type to be verified.</param>
        /// <returns>True in case the class is real, false otherwise.</returns>
        public static bool IsConcrete(this Type testType)
        {
            return testType.IsAbstract == false
                && testType.IsGenericTypeDefinition == false
                && testType.IsInterface == false;
        }

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
        /// Get property of object with type TProperty. Object must be null-able.
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

        /// <summary>
        /// Remove suffix from end
        /// </summary>
        /// <param name="str">Source string</param>
        /// <param name="suffix">Suffix to remove</param>
        /// <returns></returns>
        public static string RemoveSuffix(this string str, string suffix)
        {
            if (str.EndsWith(suffix, StringComparison.CurrentCultureIgnoreCase))
                return str.Substring(0, str.Length - suffix.Length);
            else
                return str;
        }

        // https://stackoverflow.com/a/9314733

        /// <summary>
        /// Get hierarchical structure (parent - child)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="nextItem"></param>
        /// <param name="canContinue"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
                yield return current;
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
            => FromHierarchy(source, nextItem, s => s != null);

        /// <summary>
        /// Get all distinct messages from inner <see cref="Exception"/> hierarchy
        /// </summary>
        /// <param name="x">Root <see cref="Exception"/></param>
        /// <returns><see cref="IEnumerable{sting}"/> with all distinct messages</returns>
        public static IEnumerable<string> GetAllMessages(this Exception x)
            => x.FromHierarchy(x => x.InnerException)
                .Select(x => x.Message)
                .Distinct()
                .ToList();

        /// <summary>
        /// Make first letter of string lower
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToLowerFirstChar(this string input)
        {
            string newString = input;
            if (!String.IsNullOrEmpty(newString) && Char.IsUpper(newString[0]))
                newString = Char.ToLower(newString[0]) + newString.Substring(1);
            return newString;
        }
    }
}