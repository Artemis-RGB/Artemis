using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Artemis.Models;
using Artemis.Models.Profiles;
using Newtonsoft.Json;

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
        /// <returns>All profiles matching the provided game</returns>
        public static List<ProfileModel> GetAll(GameModel game)
        {
            return GetAll().Where(g => g.GameName.Equals(game.Name)).ToList();
        }

        /// <summary>
        ///     Adds or update the given profile.
        ///     Updates occur when a profile with the same name and game exist.
        /// </summary>
        /// <param name="prof">The profile to add or update</param>
        public static void AddOrUpdate(ProfileModel prof)
        {
            if (!(prof.GameName?.Length > 1) || !(prof.KeyboardName?.Length > 1) || !(prof.Name?.Length > 1))
                throw new ArgumentException("Profile is invalid. Name, GameName and KeyboardName are required");

            var path = ProfileFolder + $@"\{prof.KeyboardName}\{prof.GameName}";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var serialized = JsonConvert.SerializeObject(prof, Formatting.Indented);
            File.WriteAllText(path + $@"\{prof.Name}.json", serialized);
        }

        private static List<ProfileModel> ReadProfiles()
        {
            CheckProfiles();
            var profiles = new List<ProfileModel>();

            // Create the directory structure
            var profileFiles = Directory.GetFiles(ProfileFolder, "*.json", SearchOption.AllDirectories);

            // Parse the JSON files into objects and add them if they are valid
            foreach (var file in profileFiles)
            {
                var prof = JsonConvert.DeserializeObject<ProfileModel>(File.ReadAllText(file));
                if (prof.GameName?.Length > 1 && prof.KeyboardName?.Length > 1 && prof.Name?.Length > 1)
                    profiles.Add(prof);
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