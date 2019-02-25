using System;

namespace Cubes.Core.Settings
{
    /// <summary>
    /// Settings Provider is a mechanism for storing and retrieving settings as POCOs
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// Load settings with given key
        /// </summary>
        /// <param name="settingsType">Settings POCO type</param>
        /// <param name="key">Settings key</param>
        /// <returns></returns>
        object Load(Type settingsType, string key);

        /// <summary>
        /// Load settings with given key
        /// </summary>
        /// <typeparam name="TSettings">Settings POCO type</typeparam>
        /// <param name="key">Settings key</param>
        /// <returns></returns>
        TSettings Load<TSettings>(string key) where TSettings : class, new();

        /// <summary>
        /// Save settings POCO
        /// </summary>
        /// <param name="settingsType">Settings POCO type</param>
        /// <param name="settingsObj">Settings POCO instance</param>
        /// <param name="key">Settings key</param>
        void Save(Type settingsType, object settingsObj, string key);

        /// <summary>
        /// Save settings POCO
        /// </summary>
        /// <typeparam name="TSettings">Settings POCO type</typeparam>
        /// <param name="settingsObj">Settings POCO instance</param>
        /// <param name="key">Settings key</param>
        void Save<TSettings>(TSettings settingsObj, string key) where TSettings : class, new();
    }

    /// <summary>
    /// ISettingsProvider shortcuts
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Load settings POCO
        /// </summary>
        /// <typeparam name="TSettings"></typeparam>
        /// <param name="settingsProvider"></param>
        /// <returns></returns>
        public static TSettings Load<TSettings>(this ISettingsProvider settingsProvider) where TSettings : class, new() =>
            settingsProvider.Load<TSettings>(String.Empty);
        /// <summary>
        /// Load settings POCO
        /// </summary>
        /// <param name="settingsProvider"></param>
        /// <param name="settingsType">Settings POCO type</param>
        /// <returns></returns>
        public static object Load(this ISettingsProvider settingsProvider, Type settingsType) =>
            settingsProvider.Load(settingsType, String.Empty);

        /// <summary>
        /// Save settings POCO
        /// </summary>
        /// <typeparam name="TSettings">Settings POCO type</typeparam>
        /// <param name="settingsProvider">Settings POCO type</param>
        /// <param name="settingsObj">Settings POCO instance</param>
        public static void Save<TSettings>(this ISettingsProvider settingsProvider, TSettings settingsObj) where TSettings : class, new() =>
            settingsProvider.Save<TSettings>(settingsObj, String.Empty);
        /// <summary>
        /// Save settings POCO
        /// </summary>
        /// <param name="settingsProvider"></param>
        /// <param name="settingsType">Settings POCO type</param>
        /// <param name="settingsObj">Settings POCO instance</param>
        public static void Save(this ISettingsProvider settingsProvider, Type settingsType, object settingsObj) =>
            settingsProvider.Save(settingsType, settingsObj, String.Empty);
    }
}


