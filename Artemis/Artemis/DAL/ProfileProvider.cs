using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Artemis.DeviceProviders;
using Artemis.Models;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Properties;
using Artemis.Utilities;
using Newtonsoft.Json;
using NLog;

namespace Artemis.DAL
{
    public static class ProfileProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string ProfileFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\profiles";

        private static readonly List<ProfileModel> Profiles = new List<ProfileModel>();
        private static bool _installedDefaults;

        /// <summary>
        ///     Get all profiles
        /// </summary>
        /// <returns>All profiles</returns>
        public static List<ProfileModel> GetAll()
        {
            lock (Profiles)
            {
                if (!Profiles.Any())
                    ReadProfiles();

                // Return a new list, this'll make sure removing/updating the retrieved list doesn't 
                // affect the datastore
                return Profiles.ToList();
            }
        }

        /// <summary>
        ///     Get all profiles matching the provided game
        /// </summary>
        /// <param name="game">The game to match</param>
        /// <param name="keyboard">The keyboard to match</param>
        /// <returns>All profiles matching the provided game</returns>
        public static List<ProfileModel> GetAll(EffectModel game, KeyboardProvider keyboard)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (keyboard == null)
                throw new ArgumentNullException(nameof(keyboard));

            return GetAll().Where(g => g.GameName.Equals(game.Name) && g.KeyboardSlug.Equals(keyboard.Slug)).ToList();
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

            lock (Profiles)
            {
                if (!Profiles.Contains(prof))
                    Profiles.Add(prof);
            }

            lock (prof)
            {
                // Store the file
                if (!(prof.GameName?.Length > 1) || !(prof.KeyboardSlug?.Length > 1) || !(prof.Name?.Length > 1))
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
                    Logger.Error(e, "Couldn't save profile '{0}.json'", prof.Name);
                    return;
                }

                File.WriteAllText(path + $@"\{prof.Name}.json", json);
            }
        }

        private static void ReadProfiles()
        {
            CheckProfiles();
            InstallDefaults();

            lock (Profiles)
            {
                Profiles.Clear();

                // Create the directory structure
                var profilePaths = Directory.GetFiles(ProfileFolder, "*.json", SearchOption.AllDirectories);

                // Parse the JSON files into objects and add them if they are valid
                foreach (var path in profilePaths)
                {
                    try
                    {
                        var prof = LoadProfileIfValid(path);
                        if (prof == null)
                            continue;

                        // Only add unique profiles
                        if (Profiles.Any(p => p.GameName == prof.GameName && p.Name == prof.Name &&
                                              p.KeyboardSlug == prof.KeyboardSlug))
                        {
                            Logger.Info("Didn't load duplicate profile: {0}", path);
                        }
                        else
                        {
                            Profiles.Add(prof);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Failed to load profile: {0} - {1}", path, e);
                    }
                }
            }
        }

        /// <summary>
        ///     Unpacks the default profiles into the profile directory
        /// </summary>
        private static void InstallDefaults()
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

            var demoProfiles = Profiles.Where(d => d.Name == "Demo (duplicate to keep changes)");
            InsertGif(demoProfiles, "GIF", Resources.demo_gif, "demo-gif");
        }

        public static void InsertGif(IEnumerable<ProfileModel> profileModels, string layerName, Bitmap gifFile,
            string fileName)
        {
            // Extract the GIF file
            var gifDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\gifs";
            Directory.CreateDirectory(gifDir);
            var gifPath = gifDir + $"\\{fileName}.gif";
            gifFile.Save(gifPath);

            foreach (var profile in profileModels)
            {
                var gifLayer = profile.GetLayers().FirstOrDefault(l => l.Name == layerName);
                if (gifLayer == null)
                    continue;

                ((KeyboardPropertiesModel) gifLayer.Properties).GifFile = gifPath;
                AddOrUpdate(profile);
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
        ///     Exports the given profile to the provided path in XML
        /// </summary>
        /// <param name="prof">The profile to export</param>
        /// <param name="path">The path to save the profile to</param>
        public static void ExportProfile(ProfileModel prof, string path)
        {
            var json = JsonConvert.SerializeObject(prof);
            File.WriteAllText(path, json);
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
            // Remove from datastore
            lock (Profiles)
            {
                // Get the profile from the datastore instead of just the provided value, to be certain it is removed
                var dsProfile = Profiles.FirstOrDefault(p => p.GameName == prof.GameName &&
                                                             p.Name == prof.Name &&
                                                             p.KeyboardSlug == prof.KeyboardSlug);
                if (dsProfile != null)
                    Profiles.Remove(dsProfile);
            }

            // Remove the file
            var path = ProfileFolder + $@"\{prof.KeyboardSlug}\{prof.GameName}\{prof.Name}.json";
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}