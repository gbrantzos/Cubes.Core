using System;

namespace Cubes.Core.Utilities
{
    public interface ITypeResolver
    {
        /// <summary>
        /// Resolve type by name, across all loaded assemblies.
        /// </summary>
        /// <param name="typeName">Type name</param>
        /// <returns>Type</returns>
        Type GetByName(string typeName);
    }
}