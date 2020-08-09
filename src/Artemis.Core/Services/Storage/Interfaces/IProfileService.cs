using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Services.Storage.Interfaces
{
    public interface IProfileService : IArtemisService
    {
        /// <summary>
        ///     Activates the last profile for each module
        /// </summary>
        void ActivateLastActiveProfiles();

        /// <summary>
        ///     Creates a new profile for the given module and returns a descriptor pointing to it
        /// </summary>
        /// <param name="module">The profile module to create the profile for</param>
        /// <param name="name">The name of the new profile</param>
        /// <returns></returns>
        ProfileDescriptor CreateProfile(ProfileModule module, string name);

        /// <summary>
        ///     Gets a descriptor for each profile stored for the given <see cref="ProfileModule" />
        /// </summary>
        /// <param name="module">The module to return profile descriptors for</param>
        /// <returns></returns>
        List<ProfileDescriptor> GetProfiles(ProfileModule module);

        /// <summary>
        ///     Writes the profile to persistent storage
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="includeChildren"></param>
        void UpdateProfile(Profile profile, bool includeChildren);

        /// <summary>
        ///     Disposes and permanently deletes the provided profile
        /// </summary>
        /// <param name="profile">The profile to delete</param>
        void DeleteProfile(Profile profile);

        /// <summary>
        ///     Activates the profile described in the given <see cref="ProfileDescriptor" /> with the currently active surface
        /// </summary>
        /// <param name="profileDescriptor">The descriptor describing the profile to activate</param>
        Profile ActivateProfile(ProfileDescriptor profileDescriptor);

        /// <summary>
        ///     Clears the active profile on the given <see cref="ProfileModule" />
        /// </summary>
        /// <param name="module">The profile module to deactivate the active profile on</param>
        void ClearActiveProfile(ProfileModule module);

        /// <summary>
        ///     Attempts to restore the profile to the state it had before the last <see cref="UpdateProfile" /> call.
        /// </summary>
        /// <param name="selectedProfile"></param>
        /// <param name="module"></param>
        bool UndoUpdateProfile(Profile selectedProfile, ProfileModule module);

        /// <summary>
        ///     Attempts to restore the profile to the state it had before the last <see cref="UndoUpdateProfile" /> call.
        /// </summary>
        /// <param name="selectedProfile"></param>
        /// <param name="module"></param>
        bool RedoUpdateProfile(Profile selectedProfile, ProfileModule module);
    }
}