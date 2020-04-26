using System;
using System.Linq;
using Cubes.Core.Commands;

namespace Cubes.Core.Utilities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequestSampleAttribute : Attribute
    {
        public Type SampleProviderType { get; }

        public RequestSampleAttribute(Type providerType)
        {
            if (!providerType.GetInterfaces().Contains(typeof(IRequestSampleProvider)))
                throw new ArgumentException($"Provider does not implement IRequestSampleProvider: {providerType.Name}");
            SampleProviderType = providerType;
        }
    }
}
