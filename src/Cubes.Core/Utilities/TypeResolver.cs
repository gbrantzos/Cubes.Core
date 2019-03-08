using System;

namespace Cubes.Core.Utilities
{
    public class TypeResolver : ITypeResolver
    {
        public Type GetByName(string typeName)
        {
            var t = Type.GetType(typeName);
            if (t == null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var a in assemblies)
                {
                    t = a.GetType(typeName);
                    if (t != null)
                        break;
                }
                if (t == null)
                    return null;
            }
            return t;
        }
    }
}