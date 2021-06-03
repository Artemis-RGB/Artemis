﻿using System;
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

        private readonly List<Exception> _updateExceptions = new();
        private DateTime _lastUpdateExceptionLog;
        private readonly List<Exception> _renderExceptions = new();
        private DateTime _lastRenderExceptionLog;

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
            _profileCategories = new List<ProfileCategory>(_profileCategoryRepository.GetAll().Select(c => new ProfileCategory(c)).OrderBy(c => c.Order));

            _rgbService.LedsChanged += RgbServiceOnLedsChanged;
            _pluginManagementService.PluginFeatureEnabled += PluginManagementServiceOnPluginFeatureToggled;
            _pluginManagementService.PluginFeatureDisabled += PluginManagementServiceOnPluginFeatureToggled;

            if (!_profileCategories.Any())
                CreateDefaultProfileCategories();
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
                foreach (ProfileConfiguration profileConfiguration in profileCategory.ProfileConfigurations)
                    profileConfiguration.LoadModules(modules);
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

        public bool RenderForEditor { get; set; }

        public void UpdateProfiles(double deltaTime)
        {
            lock (_profileCategories)
            {
                // Iterate the children in reverse because the first category must be rendered last to end up on top
                for (int i = _profileCategories.Count - 1; i > -1; i--)
                {
                    ProfileCategory profileCategory = _profileCategories[i];
                    for (int j = profileCategory.ProfileConfigurations.Count - 1; j > -1; j--)
                    {
                        ProfileConfiguration profileConfiguration = profileCategory.ProfileConfigurations[j];
                        // Profiles being edited are updated at their own leisure
                        if (profileConfiguration.IsBeingEdited)
                            continue;

                        bool shouldBeActive = profileConfiguration.ShouldBeActive(false);
                        if (shouldBeActive)
                        {
                            profileConfiguration.Update();
                            shouldBeActive = profileConfiguration.ActivationConditionMet;
                        }

                        try
                        {
                            // Make sure the profile is active or inactive according to the parameters above
                            if (shouldBeActive && profileConfiguration.Profile == null)
                                ActivateProfile(profileConfiguration);
                            else if (!shouldBeActive && profileConfiguration.Profile != null)
                                DeactivateProfile(profileConfiguration);

                            profileConfiguration.Profile?.Update(deltaTime);
                        }
                        catch (Exception e)
                        {
                            _updateExceptions.Add(e);
                        }
                    }
                }

                LogProfileUpdateExceptions();
            }
        }

        public void RenderProfiles(SKCanvas canvas)
        {
            lock (_profileCategories)
            {
                // Iterate the children in reverse because the first category must be rendered last to end up on top
                for (int i = _profileCategories.Count - 1; i > -1; i--)
                {
                    ProfileCategory profileCategory = _profileCategories[i];
                    for (int j = profileCategory.ProfileConfigurations.Count - 1; j > -1; j--)
                    {
                        try
                        {
                            ProfileConfiguration profileConfiguration = profileCategory.ProfileConfigurations[j];
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
                        catch (Exception e)
                        {
                            _renderExceptions.Add(e);
                        }
                    }
                }

                LogProfileRenderExceptions();
            }
        }

        private void CreateDefaultProfileCategories()
        {
            foreach (DefaultCategoryName defaultCategoryName in Enum.GetValues<DefaultCategoryName>())
                CreateProfileCategory(defaultCategoryName.ToString());
        }

        private void LogProfileUpdateExceptions()
        {
            // Only log update exceptions every 10 seconds to avoid spamming the logs
            if (DateTime.Now - _lastUpdateExceptionLog < TimeSpan.FromSeconds(10))
                return;
            _lastUpdateExceptionLog = DateTime.Now;

            if (!_updateExceptions.Any())
                return;

            // Group by stack trace, that should gather up duplicate exceptions
            foreach (IGrouping<string?, Exception> exceptions in _updateExceptions.GroupBy(e => e.StackTrace))
                _logger.Warning(exceptions.First(), "Exception was thrown {count} times during profile update in the last 10 seconds", exceptions.Count());

            // When logging is finished start with a fresh slate
            _updateExceptions.Clear();
        }

        private void LogProfileRenderExceptions()
        {
            // Only log update exceptions every 10 seconds to avoid spamming the logs
            if (DateTime.Now - _lastRenderExceptionLog < TimeSpan.FromSeconds(10))
                return;
            _lastRenderExceptionLog = DateTime.Now;

            if (!_renderExceptions.Any())
                return;

            // Group by stack trace, that should gather up duplicate exceptions
            foreach (IGrouping<string?, Exception> exceptions in _renderExceptions.GroupBy(e => e.StackTrace))
                _logger.Warning(exceptions.First(), "Exception was thrown {count} times during profile render in the last 10 seconds", exceptions.Count());

            // When logging is finished start with a fresh slate
            _renderExceptions.Clear();
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

        public void LoadProfileConfigurationIcon(ProfileConfiguration profileConfiguration)
        {
            if (profileConfiguration.Icon.IconType == ProfileConfigurationIconType.MaterialIcon)
                return;
            if (profileConfiguration.Icon.FileIcon != null)
                return;

            profileConfiguration.Icon.FileIcon = _profileCategoryRepository.GetProfileIconStream(profileConfiguration.Entity.FileIconId);
        }

        public void SaveProfileConfigurationIcon(ProfileConfiguration profileConfiguration)
        {
            if (profileConfiguration.Icon.IconType == ProfileConfigurationIconType.MaterialIcon)
                return;

            if (profileConfiguration.Icon.FileIcon != null)
            {
                profileConfiguration.Icon.FileIcon.Position = 0;
                _profileCategoryRepository.SaveProfileIconStream(profileConfiguration.Entity, profileConfiguration.Icon.FileIcon);
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
            ProfileConfiguration configuration = new(category, name, icon);
            ProfileEntity entity = new();
            _profileRepository.Add(entity);

            configuration.Entity.ProfileId = entity.Id;
            category.AddProfileConfiguration(configuration, 0);
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
            SaveProfileCategory(profileConfiguration.Category);
            ProfileEntity profileEntity = _profileRepository.Get(profileConfiguration.Entity.ProfileId);
            if (profileEntity != null)
                _profileRepository.Remove(profileEntity);
        }

        public void SaveProfileCategory(ProfileCategory profileCategory)
        {
            profileCategory.Save();
            _profileCategoryRepository.Save(profileCategory.Entity);

            lock (_profileCategories)
            {
                _profileCategories.Sort((a, b) => a.Order - b.Order);
            }
        }

        public void SaveProfile(Profile profile, bool includeChildren)
        {
            string memento = JsonConvert.SerializeObject(profile.ProfileEntity, IProfileService.MementoSettings);
            profile.Save();
            if (includeChildren)
            {
                foreach (Folder folder in profile.GetAllFolders())
                    folder.Save();
                foreach (Layer layer in profile.GetAllLayers())
                    layer.Save();
            }

            // If there are no changes, don't bother saving
            string updatedMemento = JsonConvert.SerializeObject(profile.ProfileEntity, IProfileService.MementoSettings);
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
                string memento = JsonConvert.SerializeObject(profile.ProfileEntity, IProfileService.MementoSettings);
                profile.RedoStack.Push(memento);
                profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top, IProfileService.MementoSettings)
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
                string memento = JsonConvert.SerializeObject(profile.ProfileEntity, IProfileService.MementoSettings);
                profile.UndoStack.Push(memento);
                profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top, IProfileService.MementoSettings)
                                        ?? throw new InvalidOperationException("Failed to deserialize memento");

                profile.Load();
                profile.PopulateLeds(_rgbService.EnabledDevices);

                _logger.Debug("Redo profile update - Success");
                return true;
            }
        }

        public ProfileConfigurationExportModel ExportProfile(ProfileConfiguration profileConfiguration)
        {
            // The profile may not be active and in that case lets activate it real quick
            Profile profile = profileConfiguration.Profile ?? ActivateProfile(profileConfiguration);

            return new ProfileConfigurationExportModel
            {
                ProfileConfigurationEntity = profileConfiguration.Entity,
                ProfileEntity = profile.ProfileEntity,
                ProfileImage = profileConfiguration.Icon.FileIcon
            };
        }

        public ProfileConfiguration ImportProfile(ProfileCategory category, ProfileConfigurationExportModel exportModel, bool makeUnique, bool markAsFreshImport, string? nameAffix)
        {
            if (exportModel.ProfileEntity == null)
                throw new ArtemisCoreException("Cannot import a profile without any data");

            // Create a copy of the entity because we'll be using it from now on
            ProfileEntity profileEntity = JsonConvert.DeserializeObject<ProfileEntity>(
                JsonConvert.SerializeObject(exportModel.ProfileEntity, IProfileService.ExportSettings), IProfileService.ExportSettings
            )!;

            // Assign a new GUID to make sure it is unique in case of a previous import of the same content
            if (makeUnique)
                profileEntity.UpdateGuid(Guid.NewGuid());

            if (nameAffix != null)
                profileEntity.Name = $"{profileEntity.Name} - {nameAffix}";
            if (markAsFreshImport)
                profileEntity.IsFreshImport = true;

            _profileRepository.Add(profileEntity);

            ProfileConfiguration profileConfiguration;
            if (exportModel.ProfileConfigurationEntity != null)
            {
                // A new GUID will be given on save
                exportModel.ProfileConfigurationEntity.FileIconId = Guid.Empty;
                profileConfiguration = new ProfileConfiguration(category, exportModel.ProfileConfigurationEntity);
                if (nameAffix != null)
                    profileConfiguration.Name = $"{profileConfiguration.Name} - {nameAffix}";
            }
            else
            {
                profileConfiguration = new ProfileConfiguration(category, exportModel.ProfileEntity!.Name, "Import");
            }

            if (exportModel.ProfileImage != null)
                profileConfiguration.Icon.FileIcon = exportModel.ProfileImage;

            profileConfiguration.Entity.ProfileId = profileEntity.Id;
            category.AddProfileConfiguration(profileConfiguration, 0);
            SaveProfileCategory(category);

            return profileConfiguration;
        }

        /// <inheritdoc />
        public void AdaptProfile(Profile profile)
        {
            string memento = JsonConvert.SerializeObject(profile.ProfileEntity, IProfileService.MementoSettings);

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