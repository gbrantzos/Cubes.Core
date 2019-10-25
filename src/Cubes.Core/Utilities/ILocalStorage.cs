namespace Cubes.Core.Utilities
{
    public interface ILocalStorage
    {
        /// <summary>
        /// Store value of type T on local storage. If value already exists it will be overwritten
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="key">Key used to retrieve</param>
        /// <param name="value">Value to store</param>
        void Save<T>(string key, T value);

        /// <summary>
        /// Get stored value with given key
        /// </summary>
        /// <param name="key">Key to retrieve</param>
        /// <returns></returns>
        object Get(string key);

        /// <summary>
        /// Get stored value with given key
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="key">Key to retrieve</param>
        /// <returns></returns>
        T Get<T>(string key) where T : class;

        /// <summary>
        /// Clear value for given key, if it does exist.
        /// </summary>
        /// <param name="key">Key to clear</param>
        void Clear(string key);
    }
}
