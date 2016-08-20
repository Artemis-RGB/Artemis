using System;
using Artemis.Settings;
using Newtonsoft.Json;

namespace Artemis.DAL
{
    public static class SettingsProvider
    {
        /// <summary>
        ///     Loads settings with the given name from the filesystem
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T Load<T>(string name)
        {
            if (!AreSettings(typeof(T)))
                throw new ArgumentException("Type doesn't implement IArtemisSettings");

            throw new NotImplementedException();
        }

        /// <summary>
        ///     Saves the settings object to the filesystem
        /// </summary>
        /// <param name="artemisSettings"></param>
        public static void Save(IArtemisSettings artemisSettings)
        {
        }

        /// <summary>
        ///     Restores the settings object to its default values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDefault<T>()
        {
            if (!AreSettings(typeof(T)))
                throw new ArgumentException("Type doesn't implement IArtemisSettings");

            // Loading the object from an empty string makes Json.NET use all the default values
            return JsonConvert.DeserializeObject<T>("");
        }

        private static bool AreSettings(Type t)
        {
            return t.IsAssignableFrom(typeof(IArtemisSettings));
        }
    }
}