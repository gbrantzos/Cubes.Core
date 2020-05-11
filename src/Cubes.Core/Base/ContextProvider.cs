using System.Threading;

namespace Cubes.Core.Base
{
    public class ContextProvider : IContextProvider
    {
        private class ContextHolder
        {
            public Context Context { get; set; }
        }

        private static readonly AsyncLocal<ContextHolder> currentContext = new AsyncLocal<ContextHolder>();

        public Context Current
        {
            get { return currentContext.Value?.Context; }
            set
            {
                var holder = currentContext.Value;
                if (holder != null)
                    holder.Context = null;

                if (value != null)
                    currentContext.Value = new ContextHolder { Context = value };
            }
        }

        public object GetData(string key)
            => Current.Data.TryGetValue(key, out object value) ? value : null;

        public void SetData(string key, object data)
        {
            if (Current.Data.ContainsKey(key))
            {
                if (data != null)
                    Current.Data[key] = data;
                else
                    Current.Data.Remove(key);
            }
            else
            {
                if (data != null)
                    Current.Data.Add(key, data);
            }
        }
    }
}
