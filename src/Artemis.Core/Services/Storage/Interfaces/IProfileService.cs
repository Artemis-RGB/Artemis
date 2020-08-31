using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.Modules;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides access to profile storage and is responsible for activating default profiles
    /// </summary>
    public interface IProfileService : IArtemisService
    {
        /// <summary>
        ///     Creates a new profile for the given module and returns a descriptor pointing to it
        /// </summary>
        /// <param name="module">The profile module to create the profile for</param>
        /// <param name="name">The name of the new profile</param>
        /// <returns></returns>
        ProfileDescriptor CreateProfileDescriptor(ProfileModule module, string name);

        /// <summary>
        ///     Gets a descriptor for each profile stored for the given <see cref="ProfileModule" />
        /// </summary>
        /// <param name="module">The module to return profile descriptors for</param>
        /// <returns></returns>
        List<ProfileDescriptor> GetProfileDescriptors(ProfileModule module);

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
        ///     Permanently deletes the profile described by the provided profile descriptor
        /// </summary>
        /// <param name="profileDescriptor">The descriptor pointing to the profile to delete</param>
        void DeleteProfile(ProfileDescriptor profileDescriptor);

        /// <summary>
        ///     Activates the last profile of the given profile module
        /// </summary>
        /// <param name="profileModule"></param>
        void ActivateLastProfile(ProfileModule profileModule);

        /// <summary>
        ///     Asynchronously activates the last profile of the given profile module using a fade animation
        /// </summary>
        /// <param name="profileModule"></param>
        /// <returns></returns>
        Task ActivateLastProfileAnimated(ProfileModule profileModule);

        /// <summary>
        ///     Activates the profile described in the given <see cref="ProfileDescriptor" /> with the currently active surface
        /// </summary>
        /// <param name="profileDescriptor">The descriptor describing the profile to activate</param>
        Profile ActivateProfile(ProfileDescriptor profileDescriptor);

        /// <summary>
        ///     Asynchronously activates the profile described in the given <see cref="ProfileDescriptor" /> with the currently
        ///     active surface using a fade animation
        /// </summary>
        /// <param name="profileDescriptor">The descriptor describing the profile to activate</param>
        Task<Profile> ActivateProfileAnimated(ProfileDescriptor profileDescriptor);

        /// <summary>
        ///     Clears the active profile on the given <see cref="ProfileModule" />
        /// </summary>
        /// <param name="module">The profile module to deactivate the active profile on</param>
        void ClearActiveProfile(ProfileModule module);

        /// <summary>
        ///     Asynchronously clears the active profile on the given <see cref="ProfileModule" /> using a fade animation
        /// </summary>
        /// <param name="module">The profile module to deactivate the active profile on</param>
        Task ClearActiveProfileAnimated(ProfileModule module);

        /// <summary>
        ///     Attempts to restore the profile to the state it had before the last <see cref="UpdateProfile" /> call.
        /// </summary>
        /// <param name="profile"></param>
        bool UndoUpdateProfile(Profile profile);

        /// <summary>
        ///     Attempts to restore the profile to the state it had before the last <see cref="UndoUpdateProfile" /> call.
        /// </summary>
        /// <param name="profile"></param>
        bool RedoUpdateProfile(Profile profile);

        /// <summary>
        ///     Prepares the profile for rendering. You should not need to call this, it is exposed for some niche usage in the
        ///     core
        /// </summary>
        /// <param name="profile"></param>
        void InstantiateProfile(Profile profile);

        /// <summary>
        ///     [Placeholder] Exports the profile described in the given <see cref="ProfileDescriptor" /> in a JSON format
        /// </summary>
        /// <param name="profileDescriptor">The descriptor of the profile to export</param>
        /// <returns>The resulting JSON</returns>
        string ExportProfile(ProfileDescriptor profileDescriptor);

        /// <summary>
        ///     [Placeholder] Imports the provided base64 encoded GZIPed JSON as a profile for the given
        ///     <see cref="ProfileModule" />
        /// </summary>
        /// <param name="json">The content of the profile as JSON</param>
        /// <param name="profileModule">The module to import the profile in to</param>
        /// <returns></returns>
        ProfileDescriptor ImportProfile(string json, ProfileModule profileModule);
    }
}