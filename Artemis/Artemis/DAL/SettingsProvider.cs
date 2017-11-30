using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Artemis.Settings;
using Artemis.Utilities;
using Newtonsoft.Json;
using NLog;

namespace Artemis.DAL
{
    public static class SettingsProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string SettingsFolder = GeneralHelpers.DataFolder + "settings\\";
        private static readonly List<IArtemisSettings> Settings = new List<IArtemisSettings>();

        /// <summary>
        ///     Loads settings with the given name from the filesystem
        /// </summary>
        /// <returns></returns>
        public static T Load<T>() where T : new()
        {
            if (!AreSettings(typeof(T)))
                throw new ArgumentException("Type doesn't implement IArtemisSettings");

            // Attempt to load from memory first
            var inMemory = Settings.FirstOrDefault(s => s.GetType() == typeof(T));
            if (inMemory != null)
                return (T) inMemory;

            CheckSettings();

            try
            {
                var loadSettings = (IArtemisSettings) JsonConvert.DeserializeObject<T>(File.ReadAllText(SettingsFolder + $"{typeof(T)}.json"));
                if (loadSettings == null)
                {
                    loadSettings = (IArtemisSettings) new T();
                    loadSettings.Reset(true);
                }

                Settings.Add(loadSettings);
                return (T) loadSettings;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Couldn't load settings '{0}.json'", typeof(T));

                // Not sure about this, I've seen prettier code
                var loadSettings = (IArtemisSettings) new T();
                loadSettings.Reset(true);
                Settings.Add(loadSettings);
                return (T) loadSettings;
            }
        }

        /// <summary>
        ///     Saves the settings object to the filesystem
        /// </summary>
        /// <param name="artemisSettings"></param>
        public static void Save(IArtemisSettings artemisSettings)
        {
            CheckSettings();

            string json;
            // Should saving fail for whatever reason, catch the exception and log it
            // But DON'T touch the settings file.
            try
            {
                json = JsonConvert.SerializeObject(artemisSettings, Formatting.Indented);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Couldn't save settings '{0}.json'", artemisSettings.GetType());
                return;
            }

            File.WriteAllText(SettingsFolder + $"{artemisSettings.GetType()}.json", json);
        }

        /// <summary>
        ///     Ensures the settings folder exists
        /// </summary>
        private static void CheckSettings()
        {
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);
        }

        /// <summary>
        ///     Checks to see if the given type is a setting
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool AreSettings(Type t)
        {
            return typeof(IArtemisSettings).IsAssignableFrom(t);
        }
    }
}