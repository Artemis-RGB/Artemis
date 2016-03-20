using System.Collections.Generic;
using System.Linq;
using Artemis.Models;

namespace Artemis.DAL
{
    internal class ProfileProvider
    {
        /// <summary>
        ///     Get all profiles
        /// </summary>
        /// <returns>All profiles</returns>
        public List<ProfileModel> GetAll()
        {
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
    }
}