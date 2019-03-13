using System;

namespace Cubes.Core.Utilities
{
    public interface ISerializer
    {
         string Serialize(object objToSerialize);

         object Deserialize(string objAsString, Type targetType);
    }

    public static class SerializerExtensions
    {
        public static T Deserialize<T>(this ISerializer serializer, string objAsString)
            => (T)serializer.Deserialize(objAsString, typeof(T));
    }
}