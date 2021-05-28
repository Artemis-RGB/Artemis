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
        ///     Gets or sets a boolean indicating whether rendering should only be done for profiles being edited
        /// </summary>
        bool RenderForEditor { get; set; }

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
        ///     Saves the provided <see cref="ProfileCategory" /> and it's <see cref="ProfileConfiguration" />s but not the
        ///     <see cref="Profile" />s themselves
        /// </summary>
        /// <param name="profileCategory">The profile category to update</param>
        void SaveProfileCategory(ProfileCategory profileCategory);

        /// <summary>
        ///     Creates a new profile category and saves it to persistent storage
        /// </summary>
        /// <param name="name">The name of the new profile category, must be unique</param>
        /// <returns>The newly created profile category</returns>
        ProfileCategory CreateProfileCategory(string name);

        /// <summary>
        ///     Permanently deletes the provided profile category
        /// </summary>
        void DeleteProfileCategory(ProfileCategory profileCategory);

        /// <summary>
        ///     Creates a new profile configuration and adds it to the provided <see cref="ProfileCategory" />
        /// </summary>
        /// <param name="category">The profile category to add the profile to</param>
        /// <param name="name">The name of the new profile configuration</param>
        /// <param name="icon">The icon of the new profile configuration</param>
        /// <returns>The newly created profile configuration</returns>
        ProfileConfiguration CreateProfileConfiguration(ProfileCategory category, string name, string icon);

        /// <summary>
        ///     Removes the provided profile configuration from the <see cref="ProfileCategory" />
        /// </summary>
        /// <param name="profileConfiguration"></param>
        void RemoveProfileConfiguration(ProfileConfiguration profileConfiguration);

        /// <summary>
        ///     Writes the profile to persistent storage
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="includeChildren"></param>
        void SaveProfile(Profile profile, bool includeChildren);

        /// <summary>
        ///     Attempts to restore the profile to the state it had before the last <see cref="SaveProfile" /> call.
        /// </summary>
        /// <param name="profile"></param>
        bool UndoSaveProfile(Profile profile);

        /// <summary>
        ///     Attempts to restore the profile to the state it had before the last <see cref="UndoSaveProfile" /> call.
        /// </summary>
        /// <param name="profile"></param>
        bool RedoSaveProfile(Profile profile);

        /// <summary>
        ///     [Placeholder] Exports the profile described in the given <see cref="ProfileDescriptor" /> in a JSON format
        /// </summary>
        /// <param name="profileConfiguration">The profile configuration of the profile to export</param>
        /// <returns>The resulting JSON</returns>
        string ExportProfile(ProfileConfiguration profileConfiguration);

        /// <summary>
        ///     [Placeholder] Imports the provided base64 encoded GZIPed JSON as a profile configuration
        /// </summary>
        /// <param name="category">The <see cref="ProfileCategory" /> in which to import the profile</param>
        /// <param name="json">The content of the profile as JSON</param>
        /// <param name="nameAffix">Text to add after the name of the profile (separated by a dash)</param>
        /// <returns></returns>
        ProfileConfiguration ImportProfile(ProfileCategory category, string json, string nameAffix = "imported");

        /// <summary>
        ///     Adapts a given profile to the currently active devices
        /// </summary>
        /// <param name="profile">The profile to adapt</param>
        void AdaptProfile(Profile profile);

        /// <summary>
        ///     Updates all currently active profiles
        /// </summary>
        void UpdateProfiles(double deltaTime);

        /// <summary>
        ///     Renders all currently active profiles
        /// </summary>
        /// <param name="canvas"></param>
        void RenderProfiles(SKCanvas canvas);
    }
}