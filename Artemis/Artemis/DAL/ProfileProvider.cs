using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Artemis.Models;
using Newtonsoft.Json;

namespace Artemis.DAL
{
    internal class ProfileProvider
    {
        private List<ProfileModel> _profiles;

        /// <summary>
        ///     Get all profiles
        /// </summary>
        /// <returns>All profiles</returns>
        public List<ProfileModel> GetAll()
        {
            ReadProfiles();
            return new List<ProfileModel>();
        }

        /// <summary>
        ///     Get all profiles matching the provided game
        /// </summary>
        /// <param name="game">The game to match</param>
        /// <returns>All profiles matching the provided game</returns>
        public List<ProfileModel> GetAll(GameModel game)
        {
            return GetAll().Where(g => g.GameName.Equals(game.Name)).ToList();
        }

        /// <summary>
        ///     Adds or update the given profile.
        ///     Updates occur when a profile with the same name and game exist.
        /// </summary>
        /// <param name="profile">The profile to add or update</param>
        public void AddOrUpdate(ProfileModel profile)
        {
        }

        private void ReadProfiles()
        {
            CheckProfiles();
            _profiles = new List<ProfileModel>();

            // Create the directory structure
            var profileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\profiles";
            var profileFiles = Directory.GetFiles(profileFolder, "*.json", SearchOption.AllDirectories);

            // Parse the JSON files into objects and add them if they are valid
            foreach (var file in profileFiles)
            {
                var prof = JsonConvert.DeserializeObject<ProfileModel>(File.ReadAllText(file));
                if (prof.GameName?.Length > 1 && prof.KeyboardName?.Length > 1 && prof.Name?.Length > 1)
                    _profiles.Add(prof);
            }
        }

        private void CheckProfiles()
        {
            // Create the directory structure
            var profileFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\profiles";
            if (Directory.Exists(profileFolder))
                return;

            Directory.CreateDirectory(profileFolder);
            Debug.WriteLine("Place presets");
        }
    }
}