using System.Collections.ObjectModel;
using Newtonsoft.Json;
using SkiaSharp;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides access to profile storage and is responsible for activating default profiles
    /// </summary>
    public interface IProfileService : IArtemisService
    {
        /// <summary>
        /// Gets the JSON serializer settings used to create profile mementos
        /// </summary>
        public static JsonSerializerSettings MementoSettings { get; } = new() {TypeNameHandling = TypeNameHandling.All};

        /// <summary>
        /// Gets the JSON serializer settings used to import/export profiles
        /// </summary>
        public static JsonSerializerSettings ExportSettings { get;  } = new() {TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented};

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
        ///     Loads the icon of this profile configuration if needed and puts it into <c>ProfileConfiguration.Icon.FileIcon</c>
        /// </summary>
        void LoadProfileConfigurationIcon(ProfileConfiguration profileConfiguration);

        /// <summary>
        ///     Saves the current icon of this profile
        /// </summary>
        void SaveProfileConfigurationIcon(ProfileConfiguration profileConfiguration);

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
        ///     Exports the profile described in the given <see cref="ProfileConfiguration" /> into an export model
        /// </summary>
        /// <param name="profileConfiguration">The profile configuration of the profile to export</param>
        /// <returns>The resulting export model</returns>
        ProfileConfigurationExportModel ExportProfile(ProfileConfiguration profileConfiguration);

        /// <summary>
        ///     Imports the provided base64 encoded GZIPed JSON as a profile configuration
        /// </summary>
        /// <param name="category">The <see cref="ProfileCategory" /> in which to import the profile</param>
        /// <param name="exportModel">The model containing the profile to import</param>
        /// <param name="makeUnique">Whether or not to give the profile a new GUID, making it unique</param>
        /// <param name="markAsFreshImport">Whether or not to mark the profile as a fresh import, causing it to be adapted until any changes are made to it</param>
        /// <param name="nameAffix">Text to add after the name of the profile (separated by a dash)</param>
        /// <returns>The resulting profile configuration</returns>
        ProfileConfiguration ImportProfile(ProfileCategory category, ProfileConfigurationExportModel exportModel, bool makeUnique = true, bool markAsFreshImport = true, string? nameAffix = "imported");

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