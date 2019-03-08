namespace Cubes.Api.RequestContext
{
    public interface IContextProvider
    {
        Context Current { get; set; }

        object GetData(string key);
        void SetData(string key, object data);
    }

    public static class ContextProviderExtensions
    {
        public static T GetData<T>(this IContextProvider contextProvider, string key) where T : class
            => contextProvider.GetData(key) as T;
        public static void ClearData(this IContextProvider contextProvider, string key)
            => contextProvider.SetData(key, null);

    }
}
