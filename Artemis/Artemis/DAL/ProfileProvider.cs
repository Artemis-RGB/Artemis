using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Artemis.DeviceProviders;
using Artemis.Models;
using Artemis.Models.Profiles;
using NLog;

namespace Artemis.DAL
{
    public static class ProfileProvider
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly string ProfileFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\profiles";

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
        public static List<ProfileModel> GetAll(GameModel game, KeyboardProvider keyboard)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            if (keyboard == null)
                throw new ArgumentNullException(nameof(keyboard));

            return GetAll().Where(g => g.GameName.Equals(game.Name) && g.KeyboardName.Equals(keyboard.Name)).ToList();
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
            if (!(prof.GameName?.Length > 1) || !(prof.KeyboardName?.Length > 1) || !(prof.Name?.Length > 1))
                throw new ArgumentException("Profile is invalid. Name, GameName and KeyboardName are required");

            var path = ProfileFolder + $@"\{prof.KeyboardName}\{prof.GameName}";
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
                        var prof = (ProfileModel)deserializer.Deserialize(file);
                        if (prof.GameName?.Length > 1 && prof.KeyboardName?.Length > 1 && prof.Name?.Length > 1)
                            profiles.Add(prof);
                    }
                }
                catch (InvalidOperationException e)
                {
                    _logger.Error("Failed to load profile: {0} - {1}", path, e.InnerException.Message);
                }
                
            }

            return profiles;
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
                    if (!(prof.GameName?.Length > 1) || !(prof.KeyboardName?.Length > 1) || !(prof.Name?.Length > 1))
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
            var path = ProfileFolder + $@"\{profile.KeyboardName}\{profile.GameName}\{profile.Name}.xml";
            if (File.Exists(path))
                File.Delete(path);

            // Update the profile, creating a new file
            profile.Name = name;
            AddOrUpdate(profile);
        }

        public static void DeleteProfile(ProfileModel profile)
        {
            // Remove the file
            var path = ProfileFolder + $@"\{profile.KeyboardName}\{profile.GameName}\{profile.Name}.xml";
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}