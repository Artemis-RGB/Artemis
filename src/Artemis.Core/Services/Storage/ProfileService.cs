using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;
using Serilog;
using SkiaSharp;

namespace Artemis.Core.Services
{
    internal class ProfileService : IProfileService
    {
        private readonly ILogger _logger;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly List<ProfileCategory> _profileCategories;
        private readonly IProfileCategoryRepository _profileCategoryRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IRgbService _rgbService;

        public ProfileService(ILogger logger,
            IRgbService rgbService,
            // TODO: Move these two
            IConditionOperatorService conditionOperatorService,
            IDataBindingService dataBindingService,
            IProfileCategoryRepository profileCategoryRepository,
            IPluginManagementService pluginManagementService,
            IProfileRepository profileRepository)
        {
            _logger = logger;
            _rgbService = rgbService;
            _profileCategoryRepository = profileCategoryRepository;
            _pluginManagementService = pluginManagementService;
            _profileRepository = profileRepository;
            _profileCategories = new List<ProfileCategory>(_profileCategoryRepository.GetAll().Select(c => new ProfileCategory(c)));

            _rgbService.LedsChanged += RgbServiceOnLedsChanged;
            _pluginManagementService.PluginFeatureEnabled += PluginManagementServiceOnPluginFeatureToggled;
            _pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureToggled;
        }

        public static JsonSerializerSettings MementoSettings { get; set; } = new() {TypeNameHandling = TypeNameHandling.All};
        public static JsonSerializerSettings ExportSettings { get; set; } = new() {TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented};
        public bool RenderForEditor { get; set; }

        public void UpdateProfiles(double deltaTime)
        {
            lock (_profileRepository)
            {
                // Not sure if nested foreach is quicker than LINQ but that seems like unnecessary overhead
                foreach (ProfileCategory profileCategory in _profileCategories)
                {
                    foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations)
                    {
                        // Profiles being edited are updated at their own leisure
                        if (profileConfiguration.IsBeingEdited)
                            continue;

                        bool shouldBeActive = profileConfiguration.ShouldBeActive(false);
                        if (shouldBeActive)
                        {
                            profileConfiguration.Update();
                            shouldBeActive = profileConfiguration.ActivationConditionMet;
                        }

                        // Make sure the profile is active or inactive according to the parameters above
                        if (shouldBeActive && profileConfiguration.Profile == null)
                            ActivateProfile(profileConfiguration);
                        else if (!shouldBeActive && profileConfiguration.Profile != null)
                            DeactivateProfile(profileConfiguration);

                        profileConfiguration.Profile?.Update(deltaTime);
                    }
                }
            }
        }

        public void RenderProfiles(SKCanvas canvas)
        {
            lock (_profileRepository)
            {
                // Not sure if nested foreach is quicker than LINQ but that seems like unnecessary overhead
                foreach (ProfileCategory profileCategory in _profileCategories)
                {
                    foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations)
                    {
                        if (RenderForEditor)
                        {
                            if (profileConfiguration.IsBeingEdited)
                                profileConfiguration.Profile?.Render(canvas, SKPointI.Empty);
                        }
                        else
                        {
                            // Ensure all criteria are met before rendering
                            if (!profileConfiguration.IsSuspended && !profileConfiguration.IsMissingModule && profileConfiguration.ActivationConditionMet)
                                profileConfiguration.Profile?.Render(canvas, SKPointI.Empty);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Populates all missing LEDs on all currently active profiles
        /// </summary>
        private void ActiveProfilesPopulateLeds()
        {
            foreach (ProfileConfiguration profileConfiguration in ProfileConfigurations)
            {
                if (profileConfiguration.Profile == null) continue;
                profileConfiguration.Profile.PopulateLeds(_rgbService.EnabledDevices);

                if (!profileConfiguration.Profile.IsFreshImport) continue;
                _logger.Debug("Profile is a fresh import, adapting to surface - {profile}", profileConfiguration.Profile);
                AdaptProfile(profileConfiguration.Profile);
            }
        }

        private void UpdateModules()
        {
            lock (_profileRepository)
            {
                List<Module> modules = _pluginManagementService.GetFeaturesOfType<Module>();
                foreach (ProfileCategory profileCategory in _profileCategories)
                {
                    foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations)
                        profileConfiguration.LoadModules(modules);
                }
            }
        }

        private void RgbServiceOnLedsChanged(object? sender, EventArgs e)
        {
            ActiveProfilesPopulateLeds();
        }

        private void PluginManagementServiceOnPluginFeatureToggled(object? sender, PluginFeatureEventArgs e)
        {
            if (e.PluginFeature is Module)
                UpdateModules();
        }

        public ReadOnlyCollection<ProfileCategory> ProfileCategories
        {
            get
            {
                lock (_profileRepository)
                {
                    return _profileCategories.AsReadOnly();
                }
            }
        }

        public ReadOnlyCollection<ProfileConfiguration> ProfileConfigurations
        {
            get
            {
                lock (_profileRepository)
                {
                    return _profileCategories.SelectMany(c => c.ProfileConfigurations).ToList().AsReadOnly();
                }
            }
        }

        public Profile ActivateProfile(ProfileConfiguration profileConfiguration)
        {
            if (profileConfiguration.Profile != null)
                return profileConfiguration.Profile;

            ProfileEntity profileEntity = _profileRepository.Get(profileConfiguration.Entity.ProfileId);
            if (profileEntity == null)
                throw new ArtemisCoreException($"Cannot find profile named: {profileConfiguration.Name} ID: {profileConfiguration.Entity.ProfileId}");

            Profile profile = new(profileConfiguration, profileEntity);
            profile.PopulateLeds(_rgbService.EnabledDevices);

            if (profile.IsFreshImport)
            {
                _logger.Debug("Profile is a fresh import, adapting to surface - {profile}", profile);
                AdaptProfile(profile);
            }

            profileConfiguration.Profile = profile;
            return profile;
        }

        public void DeactivateProfile(ProfileConfiguration profileConfiguration)
        {
            if (profileConfiguration.IsBeingEdited)
                throw new ArtemisCoreException("Cannot disable a profile that is being edited, that's rude");
            if (profileConfiguration.Profile == null)
                return;

            Profile profile = profileConfiguration.Profile;
            profileConfiguration.Profile = null;
            profile.Dispose();
        }

        public void DeleteProfile(ProfileConfiguration profileConfiguration)
        {
            DeactivateProfile(profileConfiguration);

            ProfileEntity profileEntity = _profileRepository.Get(profileConfiguration.Entity.ProfileId);
            if (profileEntity == null)
                return;

            profileConfiguration.Category.RemoveProfileConfiguration(profileConfiguration);
            _profileRepository.Remove(profileEntity);
            SaveProfileCategory(profileConfiguration.Category);
        }

        public ProfileCategory CreateProfileCategory(string name)
        {
            lock (_profileRepository)
            {
                ProfileCategory profileCategory = new(name);
                _profileCategories.Add(profileCategory);
                SaveProfileCategory(profileCategory);
                return profileCategory;
            }
        }

        public void DeleteProfileCategory(ProfileCategory profileCategory)
        {
            List<ProfileConfiguration> profileConfigurations = profileCategory.ProfileConfigurations.ToList();
            foreach (ProfileConfiguration profileConfiguration in profileConfigurations)
                RemoveProfileConfiguration(profileConfiguration);

            lock (_profileRepository)
            {
                _profileCategories.Remove(profileCategory);
                _profileCategoryRepository.Remove(profileCategory.Entity);
            }
        }

        /// <summary>
        ///     Creates a new profile configuration and adds it to the provided <see cref="ProfileCategory" />
        /// </summary>
        /// <param name="category">The profile category to add the profile to</param>
        /// <param name="name">The name of the new profile configuration</param>
        /// <param name="icon">The icon of the new profile configuration</param>
        /// <returns>The newly created profile configuration</returns>
        public ProfileConfiguration CreateProfileConfiguration(ProfileCategory category, string name, string icon)
        {
            ProfileConfiguration configuration = new(name, icon, category);
            ProfileEntity entity = new();
            _profileRepository.Add(entity);

            configuration.Entity.ProfileId = entity.Id;
            category.AddProfileConfiguration(configuration);
            return configuration;
        }

        /// <summary>
        ///     Removes the provided profile configuration from the <see cref="ProfileCategory" />
        /// </summary>
        /// <param name="profileConfiguration"></param>
        public void RemoveProfileConfiguration(ProfileConfiguration profileConfiguration)
        {
            profileConfiguration.Category.RemoveProfileConfiguration(profileConfiguration);

            DeactivateProfile(profileConfiguration);
            ProfileEntity profileEntity = _profileRepository.Get(profileConfiguration.Entity.ProfileId);
            if (profileEntity != null)
                _profileRepository.Remove(profileEntity);
        }

        public void SaveProfileCategory(ProfileCategory profileCategory)
        {
            profileCategory.Save();
            _profileCategoryRepository.Save(profileCategory.Entity);
        }

        public void SaveProfile(Profile profile, bool includeChildren)
        {
            string memento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);
            profile.Save();
            if (includeChildren)
            {
                foreach (Folder folder in profile.GetAllFolders())
                    folder.Save();
                foreach (Layer layer in profile.GetAllLayers())
                    layer.Save();
            }

            // If there are no changes, don't bother saving
            string updatedMemento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);
            if (memento.Equals(updatedMemento))
            {
                _logger.Debug("Updating profile - Skipping save, no changes");
                return;
            }

            _logger.Debug("Updating profile - Saving " + profile);
            profile.RedoStack.Clear();
            profile.UndoStack.Push(memento);

            // At this point the user made actual changes, save that
            profile.IsFreshImport = false;
            profile.ProfileEntity.IsFreshImport = false;

            _profileRepository.Save(profile.ProfileEntity);
        }

        public bool UndoSaveProfile(Profile profile)
        {
            // Keep the profile from being rendered by locking it
            lock (profile)
            {
                if (!profile.UndoStack.Any())
                {
                    _logger.Debug("Undo profile update - Failed, undo stack empty");
                    return false;
                }

                string top = profile.UndoStack.Pop();
                string memento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);
                profile.RedoStack.Push(memento);
                profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top, MementoSettings)
                                        ?? throw new InvalidOperationException("Failed to deserialize memento");

                profile.Load();
                profile.PopulateLeds(_rgbService.EnabledDevices);
            }

            _logger.Debug("Undo profile update - Success");
            return true;
        }

        public bool RedoSaveProfile(Profile profile)
        {
            // Keep the profile from being rendered by locking it
            lock (profile)
            {
                if (!profile.RedoStack.Any())
                {
                    _logger.Debug("Redo profile update - Failed, redo empty");
                    return false;
                }

                string top = profile.RedoStack.Pop();
                string memento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);
                profile.UndoStack.Push(memento);
                profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top, MementoSettings)
                                        ?? throw new InvalidOperationException("Failed to deserialize memento");

                profile.Load();
                profile.PopulateLeds(_rgbService.EnabledDevices);

                _logger.Debug("Redo profile update - Success");
                return true;
            }
        }

        public string ExportProfile(ProfileConfiguration profileConfiguration)
        {
            ProfileEntity profileEntity = _profileRepository.Get(profileConfiguration.Entity.ProfileId);
            if (profileEntity == null)
                throw new ArtemisCoreException($"Cannot find profile named: {profileConfiguration.Name} ID: {profileConfiguration.Entity.ProfileId}");

            return JsonConvert.SerializeObject(profileEntity, ExportSettings);
        }

        public ProfileConfiguration ImportProfile(ProfileCategory category, string json, string nameAffix)
        {
            ProfileEntity? profileEntity = JsonConvert.DeserializeObject<ProfileEntity>(json, ExportSettings);
            if (profileEntity == null)
                throw new ArtemisCoreException("Failed to import profile but JSON.NET threw no error :(");

            // Assign a new GUID to make sure it is unique in case of a previous import of the same content
            profileEntity.UpdateGuid(Guid.NewGuid());
            profileEntity.Name = $"{profileEntity.Name} - {nameAffix}";
            profileEntity.IsFreshImport = true;

            ProfileConfiguration configuration = new(profileEntity.Name, "Import", category);
            _profileRepository.Add(profileEntity);
            configuration.Entity.ProfileId = profileEntity.Id;
            category.AddProfileConfiguration(configuration);

            return configuration;
        }

        /// <inheritdoc />
        public void AdaptProfile(Profile profile)
        {
            string memento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);

            List<ArtemisDevice> devices = _rgbService.EnabledDevices.ToList();
            foreach (Layer layer in profile.GetAllLayers())
                layer.Adapter.Adapt(devices);

            profile.Save();

            foreach (Folder folder in profile.GetAllFolders())
                folder.Save();
            foreach (Layer layer in profile.GetAllLayers())
                layer.Save();

            _logger.Debug("Adapt profile - Saving " + profile);
            profile.RedoStack.Clear();
            profile.UndoStack.Push(memento);

            _profileRepository.Save(profile.ProfileEntity);
        }
    }
}