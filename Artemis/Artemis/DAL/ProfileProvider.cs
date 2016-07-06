﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Artemis.DeviceProviders;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Models.Profiles.Layers;
using Artemis.Properties;
using Artemis.Utilities;
using NLog;

namespace Artemis.DAL
{
    public static class ProfileProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string ProfileFolder = Environment
            .GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\profiles";

        private static bool _installedDefaults;

        /// <summary>
        ///     Get all profiles
        /// </summary>
        /// <returns>All profiles</returns>
        public static List<ProfileModel> GetAll()
        {
            return ReadProfiles();
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
            if (!(prof.GameName?.Length > 1) || !(prof.KeyboardSlug?.Length > 1) || !(prof.Name?.Length > 1))
                throw new ArgumentException("Profile is invalid. Name, GameName and KeyboardSlug are required");

            var path = ProfileFolder + $@"\{prof.KeyboardSlug}\{prof.GameName}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var serializer = new XmlSerializer(typeof(ProfileModel));

            // Could use a StreamWriter but should serializing fail this method doesn't ruin the existing XML file
            using (var xml = new StringWriter())
            {
                serializer.Serialize(xml, prof);
                File.WriteAllText(path + $@"\{prof.Name}.xml", xml.ToString());
            }
        }

        private static List<ProfileModel> ReadProfiles()
        {
            CheckProfiles();
            InstallDefaults();
            var profiles = new List<ProfileModel>();

            // Create the directory structure
            var profilePaths = Directory.GetFiles(ProfileFolder, "*.xml", SearchOption.AllDirectories);

            // Parse the JSON files into objects and add them if they are valid
            var deserializer = new XmlSerializer(typeof(ProfileModel));
            foreach (var path in profilePaths)
            {
                try
                {
                    using (var file = new StreamReader(path))
                    {
                        var prof = (ProfileModel) deserializer.Deserialize(file);
                        if (prof.GameName?.Length > 1 && prof.KeyboardSlug?.Length > 1 && prof.Name?.Length > 1)
                            profiles.Add(prof);
                    }
                }
                catch (InvalidOperationException e)
                {
                    Logger.Error("Failed to load profile: {0} - {1}", path, e.InnerException.Message);
                }
            }

            return profiles;
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

            // Extract the demo GIF file
            var gifPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\demo-gif.gif";
            Resources.demo_gif.Save(gifPath);

            // Set the GIF path on each demo profile
            var demoProfiles = GetAll().Where(d => d.Name == "Demo (Duplicate to keep changes)");
            foreach (var demoProfile in demoProfiles)
            {
                var gifLayer = demoProfile
                    .Layers.FirstOrDefault(l => l.Name == "Demo - GIFs")?
                    .Children.FirstOrDefault(c => c.Name == "GIF");

                if (gifLayer == null)
                    continue;

                ((KeyboardPropertiesModel) gifLayer.Properties).GifFile = gifPath;
                AddOrUpdate(demoProfile);
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
                var deserializer = new XmlSerializer(typeof(ProfileModel));
                using (var file = new StreamReader(path))
                {
                    var prof = (ProfileModel) deserializer.Deserialize(file);
                    if (!(prof.GameName?.Length > 1) || !(prof.KeyboardSlug?.Length > 1) || !(prof.Name?.Length > 1))
                        return null;
                    return prof;
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Exports the given profile to the provided path in XML
        /// </summary>
        /// <param name="selectedProfile">The profile to export</param>
        /// <param name="path">The path to save the profile to</param>
        public static void ExportProfile(ProfileModel selectedProfile, string path)
        {
            var serializer = new XmlSerializer(typeof(ProfileModel));
            using (var file = new StreamWriter(path))
            {
                serializer.Serialize(file, selectedProfile);
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

            // Remove the old file
            var path = ProfileFolder + $@"\{profile.KeyboardSlug}\{profile.GameName}\{profile.Name}.xml";
            if (File.Exists(path))
                File.Delete(path);

            // Update the profile, creating a new file
            profile.Name = name;
            AddOrUpdate(profile);
        }

        public static void DeleteProfile(ProfileModel profile)
        {
            // Remove the file
            var path = ProfileFolder + $@"\{profile.KeyboardSlug}\{profile.GameName}\{profile.Name}.xml";
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}