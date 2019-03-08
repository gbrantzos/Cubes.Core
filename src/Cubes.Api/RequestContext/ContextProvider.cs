using System;
using System.Threading;

namespace Cubes.Api.RequestContext
{
    public class ContextProvider : IContextProvider
    {
        private AsyncLocal<Context> context;

        public Context Current
        {
            get { return context.Value; }
            set
            {
                if (context != null)
                    throw new Exception("Context can only be set once!");
                context = new AsyncLocal<Context>();
                context.Value = value;
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
