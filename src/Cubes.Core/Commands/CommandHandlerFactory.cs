using System;
using System.Collections.Generic;

namespace Cubes.Core.Commands
{
    public delegate object CommandHandlerFactory(Type serviceType);

    public static class CommandHandlerFactoryExtensions
    {
        public static T GetInstance<T>(this CommandHandlerFactory factory)
            => (T)factory(typeof(T));

        public static IEnumerable<T> GetInstances<T>(this CommandHandlerFactory factory)
            => (IEnumerable<T>)factory(typeof(IEnumerable<T>));
    }
}