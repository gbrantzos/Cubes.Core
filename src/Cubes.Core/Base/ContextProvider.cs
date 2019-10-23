using System;
using System.Threading;

namespace Cubes.Core.Base
{
    public class ContextProvider : IContextProvider
    {
        private AsyncLocal<Context> context;

        public Context Current
        {
            get => context.Value;
            set
            {
                if (context != null)
                    throw new Exception("Context can only be set once!");
                context = new AsyncLocal<Context>
                {
                    Value = value
                };
            }
        }

        public object GetData(string key)
            => context.Value.Data.TryGetValue(key, out object value) ? value : null;

        public void SetData(string key, object data)
        {
            if (context.Value.Data.ContainsKey(key))
            {
                if (data != null)
                    context.Value.Data[key] = data;
                else
                    context.Value.Data.Remove(key);
            }
            else
            {
                if (data != null)
                    context.Value.Data.Add(key, data);
            }
        }
    }
}
