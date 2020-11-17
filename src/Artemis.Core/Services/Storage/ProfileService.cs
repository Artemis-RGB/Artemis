using System;
using System.Collections.Generic;
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
        private readonly IProfileRepository _profileRepository;
        private readonly ISurfaceService _surfaceService;

        public ProfileService(ILogger logger,
            IPluginManagementService pluginManagementService,
            ISurfaceService surfaceService,
            IConditionOperatorService conditionOperatorService,
            IDataBindingService dataBindingService,
            IProfileRepository profileRepository)
        {
            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _surfaceService = surfaceService;
            _profileRepository = profileRepository;

            _surfaceService.ActiveSurfaceConfigurationSelected += OnActiveSurfaceConfigurationSelected;
            _surfaceService.SurfaceConfigurationUpdated += OnSurfaceConfigurationUpdated;
        }

        public JsonSerializerSettings MementoSettings { get; set; } = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
        public JsonSerializerSettings ExportSettings { get; set; } = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented};

        public List<ProfileDescriptor> GetProfileDescriptors(ProfileModule module)
        {
            List<ProfileEntity> profileEntities = _profileRepository.GetByModuleId(module.Id);
            return profileEntities.Select(e => new ProfileDescriptor(module, e)).ToList();
        }

        public ProfileDescriptor CreateProfileDescriptor(ProfileModule module, string name)
        {
            ProfileEntity profileEntity = new ProfileEntity {Id = Guid.NewGuid(), Name = name, ModuleId = module.Id};
            _profileRepository.Add(profileEntity);

            return new ProfileDescriptor(module, profileEntity);
        }

        public void ActivateLastProfile(ProfileModule profileModule)
        {
            ProfileDescriptor activeProfile = GetLastActiveProfile(profileModule);
            if (activeProfile != null)
                ActivateProfile(activeProfile);
        }

        public async Task ActivateLastProfileAnimated(ProfileModule profileModule)
        {
            ProfileDescriptor activeProfile = GetLastActiveProfile(profileModule);
            if (activeProfile != null)
                await ActivateProfileAnimated(activeProfile);
        }

        public Profile ActivateProfile(ProfileDescriptor profileDescriptor)
        {
            if (profileDescriptor.ProfileModule.ActiveProfile?.EntityId == profileDescriptor.Id)
                return profileDescriptor.ProfileModule.ActiveProfile;

            ProfileEntity profileEntity = _profileRepository.Get(profileDescriptor.Id);
            if (profileEntity == null)
                throw new ArtemisCoreException($"Cannot find profile named: {profileDescriptor.Name} ID: {profileDescriptor.Id}");

            Profile profile = new Profile(profileDescriptor.ProfileModule, profileEntity);
            InstantiateProfile(profile);

            profileDescriptor.ProfileModule.ChangeActiveProfile(profile, _surfaceService.ActiveSurface);
            SaveActiveProfile(profileDescriptor.ProfileModule);

            return profile;
        }

        public void ReloadProfile(ProfileModule module)
        {
            if (module.ActiveProfile == null)
                return;

            ProfileEntity entity = _profileRepository.Get(module.ActiveProfile.EntityId);
            Profile profile = new Profile(module, entity);
            InstantiateProfile(profile);

            module.ChangeActiveProfile(null, _surfaceService.ActiveSurface);
            module.ChangeActiveProfile(profile, _surfaceService.ActiveSurface);
        }

        public async Task<Profile> ActivateProfileAnimated(ProfileDescriptor profileDescriptor)
        {
            if (profileDescriptor.ProfileModule.ActiveProfile?.EntityId == profileDescriptor.Id)
                return profileDescriptor.ProfileModule.ActiveProfile;

            ProfileEntity profileEntity = _profileRepository.Get(profileDescriptor.Id);
            if (profileEntity == null)
                throw new ArtemisCoreException($"Cannot find profile named: {profileDescriptor.Name} ID: {profileDescriptor.Id}");

            Profile profile = new Profile(profileDescriptor.ProfileModule, profileEntity);
            InstantiateProfile(profile);

            void ActivatingProfileSurfaceUpdate(object? sender, SurfaceConfigurationEventArgs e)
            {
                profile.PopulateLeds(e.Surface);
            }

            void ActivatingProfilePluginToggle(object? sender, PluginEventArgs e)
            {
                InstantiateProfile(profile);
            }

            // This could happen during activation so subscribe to it
            _pluginManagementService.PluginEnabled += ActivatingProfilePluginToggle;
            _pluginManagementService.PluginDisabled += ActivatingProfilePluginToggle;
            _surfaceService.SurfaceConfigurationUpdated += ActivatingProfileSurfaceUpdate;

            await profileDescriptor.ProfileModule.ChangeActiveProfileAnimated(profile, _surfaceService.ActiveSurface);
            SaveActiveProfile(profileDescriptor.ProfileModule);

            _pluginManagementService.PluginEnabled -= ActivatingProfilePluginToggle;
            _pluginManagementService.PluginDisabled -= ActivatingProfilePluginToggle;
            _surfaceService.SurfaceConfigurationUpdated -= ActivatingProfileSurfaceUpdate;

            return profile;
        }


        public void ClearActiveProfile(ProfileModule module)
        {
            module.ChangeActiveProfile(null, _surfaceService.ActiveSurface);
            SaveActiveProfile(module);
        }

        public async Task ClearActiveProfileAnimated(ProfileModule module)
        {
            await module.ChangeActiveProfileAnimated(null, _surfaceService.ActiveSurface);
        }

        public void DeleteProfile(Profile profile)
        {
            _logger.Debug("Removing profile " + profile);

            // If the given profile is currently active, disable it first (this also disposes it)
            if (profile.Module.ActiveProfile == profile)
                ClearActiveProfile(profile.Module);
            else
                profile.Dispose();

            _profileRepository.Remove(profile.ProfileEntity);
        }

        public void DeleteProfile(ProfileDescriptor profileDescriptor)
        {
            ProfileEntity profileEntity = _profileRepository.Get(profileDescriptor.Id);
            _profileRepository.Remove(profileEntity);
        }

        public void UpdateProfile(Profile profile, bool includeChildren)
        {
            _logger.Debug("Updating profile " + profile);
            string memento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);
            profile.RedoStack.Clear();
            profile.UndoStack.Push(memento);

            profile.Save();
            if (includeChildren)
            {
                foreach (Folder folder in profile.GetAllFolders())
                    folder.Save();
                foreach (Layer layer in profile.GetAllLayers())
                    layer.Save();
            }

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
                profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top, MementoSettings);

                profile.Load();
                InstantiateProfile(profile);
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
                profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top, MementoSettings);

                profile.Load();
                InstantiateProfile(profile);

                _logger.Debug("Redo profile update - Success");
                return true;
            }
        }

        public void InstantiateProfile(Profile profile)
        {
            profile.PopulateLeds(_surfaceService.ActiveSurface);
        }

        public string ExportProfile(ProfileDescriptor profileDescriptor)
        {
            ProfileEntity profileEntity = _profileRepository.Get(profileDescriptor.Id);
            if (profileEntity == null)
                throw new ArtemisCoreException($"Cannot find profile named: {profileDescriptor.Name} ID: {profileDescriptor.Id}");

            return JsonConvert.SerializeObject(profileEntity, ExportSettings);
        }

        public ProfileDescriptor ImportProfile(string json, ProfileModule profileModule)
        {
            ProfileEntity profileEntity = JsonConvert.DeserializeObject<ProfileEntity>(json, ExportSettings);

            // Assign a new GUID to make sure it is unique in case of a previous import of the same content
            profileEntity.UpdateGuid(Guid.NewGuid());
            profileEntity.Name = $"{profileEntity.Name} - Imported";

            _profileRepository.Add(profileEntity);
            return new ProfileDescriptor(profileModule, profileEntity);
        }

        public ProfileDescriptor GetLastActiveProfile(ProfileModule module)
        {
            List<ProfileEntity> moduleProfiles = _profileRepository.GetByModuleId(module.Id);
            if (!moduleProfiles.Any())
                return CreateProfileDescriptor(module, "Default");

            ProfileEntity profileEntity = moduleProfiles.FirstOrDefault(p => p.IsActive) ?? moduleProfiles.FirstOrDefault();
            return profileEntity == null ? null : new ProfileDescriptor(module, profileEntity);
        }

        private void SaveActiveProfile(ProfileModule module)
        {
            if (module.ActiveProfile == null)
                return;

            List<ProfileEntity> profileEntities = _profileRepository.GetByModuleId(module.Id);
            foreach (ProfileEntity profileEntity in profileEntities)
            {
                profileEntity.IsActive = module.ActiveProfile.EntityId == profileEntity.Id;
                _profileRepository.Save(profileEntity);
            }
        }

        /// <summary>
        ///     Populates all missing LEDs on all currently active profiles
        /// </summary>
        /// <param name="surface"></param>
        private void ActiveProfilesPopulateLeds(ArtemisSurface surface)
        {
            List<ProfileModule> profileModules = _pluginManagementService.GetFeaturesOfType<ProfileModule>();
            foreach (ProfileModule profileModule in profileModules.Where(p => p.ActiveProfile != null).ToList())
                profileModule.ActiveProfile.PopulateLeds(surface);
        }

        #region Event handlers

        private void OnActiveSurfaceConfigurationSelected(object? sender, SurfaceConfigurationEventArgs e)
        {
            ActiveProfilesPopulateLeds(e.Surface);
        }

        private void OnSurfaceConfigurationUpdated(object? sender, SurfaceConfigurationEventArgs e)
        {
            if (e.Surface.IsActive)
                ActiveProfilesPopulateLeds(e.Surface);
        }

        #endregion
    }
}