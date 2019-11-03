using System;

namespace Cubes.Core.Configuration
{
    public interface IConfigurationWriter
    {
        void Save(Type configType, object configInstance);
        void Save(string configTypeName, object configInstance);
    }
}
