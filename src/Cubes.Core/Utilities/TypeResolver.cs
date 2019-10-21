using System;

namespace Cubes.Core.Utilities
{
    public class TypeResolver : ITypeResolver
    {
        public Type GetByName(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    type = asm.GetType(typeName);
                    if (type != null)
                        break;
                }
                if (type == null)
                    return null;
            }
            return type;
        }
    }
}