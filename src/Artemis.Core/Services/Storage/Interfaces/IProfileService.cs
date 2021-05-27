using System.Collections.ObjectModel;
using SkiaSharp;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides access to profile storage and is responsible for activating default profiles
    /// </summary>
    public interface IProfileService : IArtemisService
    {
        /// <summary>
        ///     Gets a read only collection containing all the profile categories
        /// </summary>
        ReadOnlyCollection<ProfileCategory> ProfileCategories { get; }

        /// <summary>
        ///     Gets a read only collection containing all the profile configurations
        /// </summary>
        ReadOnlyCollection<ProfileConfiguration> ProfileConfigurations { get; }

        /// <summary>
        ///     Activates the profile of the given <see cref="ProfileConfiguration" /> with the currently active surface
        /// </summary>
        /// <param name="profileConfiguration">The profile configuration of the profile to activate</param>
        Profile ActivateProfile(ProfileConfiguration profileConfiguration);

        /// <summary>
        ///     Deactivates the profile of the given <see cref="ProfileConfiguration" /> with the currently active surface
        /// </summary>
        /// <param name="profileConfiguration">The profile configuration of the profile to activate</param>
        void DeactivateProfile(ProfileConfiguration profileConfiguration);

        /// <summary>
        ///     Permanently deletes the profile of the given <see cref="ProfileConfiguration" />
        /// </summary>
        /// <param name="profileConfiguration">The profile configuration of the profile to delete</param>
        void DeleteProfile(ProfileConfiguration profileConfiguration);

        /// <summary>
        ///     Updates the provided <see cref="ProfileCategory" /> and it's <see cref="ProfileConfiguration"/>s but not the <see cref="Profile" />s themselves
        /// </summary>
        /// <param name="profileCategory">The profile category to update</param>
        void UpdateProfileCategory(ProfileCategory profileCategory);

        /// <summary>
        ///     Writes the profile to persistent storage
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="includeChildren"></param>
        void UpdateProfile(Profile profile, bool includeChildren);

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
        ///     [Placeholder] Exports the profile described in the given <see cref="ProfileDescriptor" /> in a JSON format
        /// </summary>
        /// <param name="profileConfiguration">The profile configuration of the profile to export</param>
        /// <returns>The resulting JSON</returns>
        string ExportProfile(ProfileConfiguration profileConfiguration);

        /// <summary>
        ///     [Placeholder] Imports the provided base64 encoded GZIPed JSON as a profile configuration
        /// </summary>
        /// <param name="category">The <see cref="ProfileCategory"/> in which to import the profile</param>
        /// <param name="json">The content of the profile as JSON</param>
        /// <param name="nameAffix">Text to add after the name of the profile (separated by a dash)</param>
        /// <returns></returns>
        ProfileConfiguration ImportProfile(ProfileCategory category, string json, string nameAffix = "imported");

        /// <summary>
        ///     Adapts a given profile to the currently active devices
        /// </summary>
        /// <param name="profile">The profile to adapt</param>
        void AdaptProfile(Profile profile);

        void UpdateProfiles(double deltaTime);
        void RenderProfiles(SKCanvas canvas);
    }
}