using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Artemis.DeviceProviders;
using Artemis.Models;
using Artemis.Models.Profiles;

namespace Artemis.DAL
{
    public static class ProfileProvider
    {
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
            using (var file = new StreamWriter(path + $@"\{prof.Name}.xml"))
            {
                serializer.Serialize(file, prof);
            }
        }

        private static List<ProfileModel> ReadProfiles()
        {
            CheckProfiles();
            var profiles = new List<ProfileModel>();

            // Create the directory structure
            var profilePaths = Directory.GetFiles(ProfileFolder, "*.xml", SearchOption.AllDirectories);

            // Parse the JSON files into objects and add them if they are valid
            // TODO: Invalid file handling
            var deserializer = new XmlSerializer(typeof(ProfileModel));
            foreach (var path in profilePaths)
            {
                using (var file = new StreamReader(path))
                {
                    var prof = (ProfileModel) deserializer.Deserialize(file);
                    if (prof.GameName?.Length > 1 && prof.KeyboardName?.Length > 1 && prof.Name?.Length > 1)
                        profiles.Add(prof);
                }
            }

            return profiles;
        }

        private static void CheckProfiles()
        {
            // Create the directory structure
            if (Directory.Exists(ProfileFolder))
                return;

            Directory.CreateDirectory(ProfileFolder);
            Debug.WriteLine("Place presets");
        }
    }
}