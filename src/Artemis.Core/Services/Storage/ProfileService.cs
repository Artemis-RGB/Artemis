using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace Artemis.Core.Services
{
    internal class ProfileService : IProfileService
    {
        private readonly ILogger _logger;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IRgbService _rgbService;
        private readonly IProfileCategoryRepository _profileCategoryRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly List<ProfileCategory> _profileCategories;

        public ProfileService(ILogger logger,
            IPluginManagementService pluginManagementService,
            IRgbService rgbService,
            IConditionOperatorService conditionOperatorService,
            IDataBindingService dataBindingService,
            IProfileCategoryRepository profileCategoryRepository,
            IProfileRepository profileRepository)
        {
            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _rgbService = rgbService;
            _profileCategoryRepository = profileCategoryRepository;
            _profileRepository = profileRepository;
            _profileCategories = new List<ProfileCategory>(_profileCategoryRepository.GetAll().Select(c => new ProfileCategory(c)));

            _rgbService.LedsChanged += RgbServiceOnLedsChanged;
        }

        public static JsonSerializerSettings MementoSettings { get; set; } = new() {TypeNameHandling = TypeNameHandling.All};
        public static JsonSerializerSettings ExportSettings { get; set; } = new() {TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented};

        public ReadOnlyCollection<ProfileCategory> ProfileCategories => _profileCategories.AsReadOnly();
        public ReadOnlyCollection<ProfileConfiguration> ProfileConfigurations => _profileCategories.SelectMany(c => c.ProfileConfigurations).ToList().AsReadOnly();

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
            UpdateProfileCategory(profileConfiguration.Category);
        }

        public void UpdateProfileCategory(ProfileCategory profileCategory)
        {
            profileCategory.Save();
            _profileCategoryRepository.Save(profileCategory.Entity);
        }

        public void UpdateProfile(Profile profile, bool includeChildren)
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

        public bool UndoUpdateProfile(Profile profile)
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

        public bool RedoUpdateProfile(Profile profile)
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

            _profileRepository.Add(profileEntity);
            ProfileConfiguration profileConfiguration = category.AddProfileConfiguration(profileEntity.Name, "Import");
            profileConfiguration.Entity.ProfileId = profileEntity.Id;

            return profileConfiguration;
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

        #region Event handlers

        private void RgbServiceOnLedsChanged(object? sender, EventArgs e)
        {
            ActiveProfilesPopulateLeds();
        }

        #endregion
    }
}