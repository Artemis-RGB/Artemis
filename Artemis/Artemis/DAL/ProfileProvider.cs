using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Artemis.DeviceProviders;
using Artemis.Modules.Abstract;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Properties;
using Artemis.Utilities;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using NLog;

namespace Artemis.DAL
{
    public static class ProfileProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static readonly string ProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\profiles";

        private static bool _installedDefaults;

        static ProfileProvider()
        {
            // Configure MoonSharp
            UserData.RegisterAssembly();
            CheckProfiles();
            InstallDefaults();
        }
        
        public static List<string> GetProfileNames(KeyboardProvider keyboard, ModuleModel module)
        {
            if (keyboard == null || module == null)
                return null;
            return ReadProfiles(keyboard.Slug + "/" + module.Name).Select(p => p.Name).ToList();
        }

        public static ProfileModel GetProfile(KeyboardProvider keyboard, ModuleModel module, string name)
        {
            if (keyboard == null || module == null)
                return null;
            return ReadProfiles(keyboard.Slug + "/" + module.Name).FirstOrDefault(p => p.Name == name);
        }

        public static bool IsProfileUnique(ProfileModel profileModel)
        {
            var existing = ReadProfiles(profileModel.KeyboardSlug + "/" + profileModel.GameName);
            return !existing.Contains(profileModel);
        }

        /// <summary>
        ///     Adds or update the given profile.
        ///     Updates occur when a profile with the same name and game exist.
        /// </summary>
        /// <param name="prof">The profile to add or update</param>
        public static void AddOrUpdate(ProfileModel prof)
        {
            if (prof == null)
                throw new ArgumentNullException(nameof(prof));

            lock (prof)
            {
                // Store the file
                if (!(prof.GameName?.Length > 1) || !(prof.KeyboardSlug?.Length > 1) || !(prof.Slug?.Length > 1))
                    throw new ArgumentException("Profile is invalid. Name, GameName and KeyboardSlug are required");

                var path = ProfileFolder + $@"\{prof.KeyboardSlug}\{prof.GameName}";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string json;

                // Should saving fail for whatever reason, catch the exception and log it
                // But DON'T touch the profile file.
                try
                {
                    json = JsonConvert.SerializeObject(prof, Formatting.Indented);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Couldn't save profile '{0}.json'", prof.Slug);
                    return;
                }

                File.WriteAllText(path + $@"\{prof.Slug}.json", json);
                Logger.Debug("Saved profile {0}/{1}/{2}", prof.KeyboardSlug, prof.GameName, prof.Name);
            }
        }

        /// <summary>
        ///     Renames the profile on the model and filesystem
        /// </summary>
        /// <param name="profile">The profile to rename</param>
        /// <param name="name">The new name</param>
        public static void RenameProfile(ProfileModel profile, string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            // Remove the old profile
            DeleteProfile(profile);

            // Update the profile, creating a new file
            profile.Name = name;
            AddOrUpdate(profile);
        }

        public static void DeleteProfile(ProfileModel prof)
        {
            // Remove the file
            var path = ProfileFolder + $@"\{prof.KeyboardSlug}\{prof.GameName}\{prof.Slug}.json";
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        ///     Attempts to load a profile from a given path
        /// </summary>
        /// <param name="path">The absolute path to load the profile from</param>
        /// <returns>The loaded profile, or null if invalid</returns>
        public static ProfileModel LoadProfileIfValid(string path)
        {
            try
            {
                var prof = JsonConvert.DeserializeObject<ProfileModel>(File.ReadAllText(path));
                if (prof == null)
                    return null;
                if (prof.GameName.Length < 1 || prof.KeyboardSlug.Length < 1 || prof.Name.Length < 1)
                    return null;
                return prof;
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Exports the given profile to the provided path in JSON 
        /// </summary>
        /// <param name="prof">The profile to export</param>
        /// <param name="path">The path to save the profile to</param>
        public static void ExportProfile(ProfileModel prof, string path)
        {
            var json = JsonConvert.SerializeObject(prof);
            File.WriteAllText(path, json);
        }

        public static void InsertGif(string effectName, string profileName, string layerName, Bitmap gifFile,
            string fileName)
        {
            var directories = new DirectoryInfo(ProfileFolder).GetDirectories();
            var profiles = new List<ProfileModel>();
            foreach (var directoryInfo in directories)
                profiles.AddRange(ReadProfiles(directoryInfo.Name + "/effectName").Where(d => d.Name == profileName));

            // Extract the GIF file
            var gifDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\gifs";
            Directory.CreateDirectory(gifDir);
            var gifPath = gifDir + $"\\{fileName}.gif";
            gifFile.Save(gifPath);

            foreach (var profile in profiles)
            {
                var gifLayer = profile.GetLayers().FirstOrDefault(l => l.Name == layerName);
                if (gifLayer == null)
                    continue;

                ((KeyboardPropertiesModel) gifLayer.Properties).GifFile = gifPath;
                AddOrUpdate(profile);
            }
        }

        public static List<ProfileModel> ReadProfiles(string subDirectory)
        {
            var profiles = new List<ProfileModel>();
            var directory = ProfileFolder + "/" + subDirectory;
            if (!Directory.Exists(directory))
                return profiles;

            // Create the directory structure
            var profilePaths = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories);

            // Parse the JSON files into objects and add them if they are valid
            foreach (var path in profilePaths)
            {
                try
                {
                    var prof = LoadProfileIfValid(path);
                    if (prof == null)
                        continue;

                    // Only add unique profiles
                    if (profiles.Any(p => p.GameName == prof.GameName && p.Name == prof.Name &&
                                          p.KeyboardSlug == prof.KeyboardSlug))
                    {
                        Logger.Info("Didn't load duplicate profile: {0}", path);
                    }
                    else
                    {
                        profiles.Add(prof);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Failed to load profile: {0} - {1}", path, e);
                }
            }
            return profiles;
        }

        /// <summary>
        ///     Unpacks the default profiles into the profile directory
        /// </summary>
        private static void InstallDefaults()
        {
            try
            {
                // Only install the defaults once per session
                if (_installedDefaults)
                    return;
                _installedDefaults = true;

                // Load the ZIP from resources
                var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Artemis.Resources.Keyboards.default-profiles.zip");

                // Extract it over the old defaults in case one was updated
                if (stream == null)
                    return;
                var archive = new ZipArchive(stream);
                archive.ExtractToDirectory(ProfileFolder, true);


                InsertGif("GeneralProfile", "Demo (duplicate to keep changes)", "GIF", Resources.demo_gif, "demo-gif");
            }
            catch (IOException e)
            {
                Logger.Warn(e, "Failed to place default profiles, perhaps there are two instances of Artemis " +
                               "starting at the same time?");
            }
        }

        /// <summary>
        ///     Makes sure the profile directory structure is in order and places default profiles
        /// </summary>
        private static void CheckProfiles()
        {
            // Create the directory structure
            if (Directory.Exists(ProfileFolder))
                return;

            Directory.CreateDirectory(ProfileFolder);
        }
    }
}