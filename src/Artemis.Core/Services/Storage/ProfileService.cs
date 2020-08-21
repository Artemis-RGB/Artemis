using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.LayerBrushes;
using Artemis.Core.Plugins.LayerEffects;
using Artemis.Core.Plugins.Modules;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace Artemis.Core.Services.Storage
{
    internal class ProfileService : IProfileService
    {
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;
        private readonly IProfileRepository _profileRepository;
        private readonly IRenderElementService _renderElementService;
        private readonly ISurfaceService _surfaceService;

        internal ProfileService(ILogger logger, IPluginService pluginService, ISurfaceService surfaceService, IRenderElementService renderElementService, IProfileRepository profileRepository)
        {
            _logger = logger;
            _pluginService = pluginService;
            _surfaceService = surfaceService;
            _renderElementService = renderElementService;
            _profileRepository = profileRepository;

            _surfaceService.ActiveSurfaceConfigurationSelected += OnActiveSurfaceConfigurationSelected;
            _surfaceService.SurfaceConfigurationUpdated += OnSurfaceConfigurationUpdated;
            _pluginService.PluginEnabled += OnPluginToggled;
            _pluginService.PluginDisabled += OnPluginToggled;
        }

        public JsonSerializerSettings MementoSettings { get; set; } = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
        public JsonSerializerSettings ExportSettings { get; set; } = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented};

        public List<ProfileDescriptor> GetProfileDescriptors(ProfileModule module)
        {
            var profileEntities = _profileRepository.GetByPluginGuid(module.PluginInfo.Guid);
            return profileEntities.Select(e => new ProfileDescriptor(module, e)).ToList();
        }

        public ProfileDescriptor CreateProfileDescriptor(ProfileModule module, string name)
        {
            var profileEntity = new ProfileEntity {Id = Guid.NewGuid(), Name = name, PluginGuid = module.PluginInfo.Guid};
            _profileRepository.Add(profileEntity);

            return new ProfileDescriptor(module, profileEntity);
        }

        public void ActivateLastProfile(ProfileModule profileModule)
        {
            var activeProfile = GetLastActiveProfile(profileModule);
            if (activeProfile != null)
                ActivateProfile(activeProfile);
        }

        public Profile ActivateProfile(ProfileDescriptor profileDescriptor)
        {
            if (profileDescriptor.ProfileModule.ActiveProfile?.EntityId == profileDescriptor.Id)
                return profileDescriptor.ProfileModule.ActiveProfile;

            var profileEntity = _profileRepository.Get(profileDescriptor.Id);
            if (profileEntity == null)
                throw new ArtemisCoreException($"Cannot find profile named: {profileDescriptor.Name} ID: {profileDescriptor.Id}");

            var profile = new Profile(profileDescriptor.ProfileModule, profileEntity);
            InstantiateProfile(profile);

            profileDescriptor.ProfileModule.ChangeActiveProfile(profile, _surfaceService.ActiveSurface);
            SaveActiveProfile(profileDescriptor.ProfileModule);

            return profile;
        }

        public async Task<Profile> ActivateProfileAnimated(ProfileDescriptor profileDescriptor)
        {
            if (profileDescriptor.ProfileModule.ActiveProfile?.EntityId == profileDescriptor.Id)
                return profileDescriptor.ProfileModule.ActiveProfile;

            var profileEntity = _profileRepository.Get(profileDescriptor.Id);
            if (profileEntity == null)
                throw new ArtemisCoreException($"Cannot find profile named: {profileDescriptor.Name} ID: {profileDescriptor.Id}");

            var profile = new Profile(profileDescriptor.ProfileModule, profileEntity);
            InstantiateProfile(profile);

            await profileDescriptor.ProfileModule.ChangeActiveProfileAnimated(profile, _surfaceService.ActiveSurface);
            SaveActiveProfile(profileDescriptor.ProfileModule);

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
            var profileEntity = _profileRepository.Get(profileDescriptor.Id);
            _profileRepository.Remove(profileEntity);
        }

        public void UpdateProfile(Profile profile, bool includeChildren)
        {
            _logger.Debug("Updating profile " + profile);
            var memento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);
            profile.RedoStack.Clear();
            profile.UndoStack.Push(memento);

            profile.ApplyToEntity();
            if (includeChildren)
            {
                foreach (var folder in profile.GetAllFolders())
                    folder.ApplyToEntity();
                foreach (var layer in profile.GetAllLayers())
                    layer.ApplyToEntity();
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

                var top = profile.UndoStack.Pop();
                var memento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);
                profile.RedoStack.Push(memento);
                profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top, MementoSettings);

                profile.ApplyToProfile();
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

                var top = profile.RedoStack.Pop();
                var memento = JsonConvert.SerializeObject(profile.ProfileEntity, MementoSettings);
                profile.UndoStack.Push(memento);
                profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top, MementoSettings);

                profile.ApplyToProfile();
                InstantiateProfile(profile);

                _logger.Debug("Redo profile update - Success");
                return true;
            }
        }

        public void InstantiateProfile(Profile profile)
        {
            profile.PopulateLeds(_surfaceService.ActiveSurface);
            InitializeLayerProperties(profile);
            InstantiateLayers(profile);
            InstantiateFolders(profile);
        }

        public string ExportProfile(ProfileDescriptor profileDescriptor)
        {
            var profileEntity = _profileRepository.Get(profileDescriptor.Id);
            if (profileEntity == null)
                throw new ArtemisCoreException($"Cannot find profile named: {profileDescriptor.Name} ID: {profileDescriptor.Id}");

            return JsonConvert.SerializeObject(profileEntity, ExportSettings);
        }

        public ProfileDescriptor ImportProfile(string json, ProfileModule profileModule)
        {
            var profileEntity = JsonConvert.DeserializeObject<ProfileEntity>(json, ExportSettings);

            // Assign a new GUID to make sure it is unique in case of a previous import of the same content
            profileEntity.Id = Guid.NewGuid();

            _profileRepository.Add(profileEntity);
            return new ProfileDescriptor(profileModule, profileEntity);
        }

        public ProfileDescriptor GetLastActiveProfile(ProfileModule module)
        {
            var moduleProfiles = _profileRepository.GetByPluginGuid(module.PluginInfo.Guid);
            var profileEntity = moduleProfiles.FirstOrDefault(p => p.IsActive) ?? moduleProfiles.FirstOrDefault();
            return profileEntity == null ? null : new ProfileDescriptor(module, profileEntity);
        }

        private void SaveActiveProfile(ProfileModule module)
        {
            var profileEntities = _profileRepository.GetByPluginGuid(module.PluginInfo.Guid);
            foreach (var profileEntity in profileEntities)
            {
                profileEntity.IsActive = module.ActiveProfile.EntityId == profileEntity.Id;
                _profileRepository.Save(profileEntity);
            }
        }

        /// <summary>
        ///     Initializes the properties on the layers of the given profile
        /// </summary>
        /// <param name="profile"></param>
        private void InitializeLayerProperties(Profile profile)
        {
            foreach (var layer in profile.GetAllLayers())
            {
                if (!layer.General.PropertiesInitialized)
                    layer.General.InitializeProperties(_renderElementService, layer, "General.");
                if (!layer.Transform.PropertiesInitialized)
                    layer.Transform.InitializeProperties(_renderElementService, layer, "Transform.");
            }
        }

        /// <summary>
        ///     Instantiates all plugin-related classes on the folders of the given profile
        /// </summary>
        private void InstantiateFolders(Profile profile)
        {
            foreach (var folder in profile.GetAllFolders())
            {
                // Instantiate effects
                _renderElementService.InstantiateLayerEffects(folder);
                // Remove effects of plugins that are disabled
                var disabledEffects = new List<BaseLayerEffect>(folder.LayerEffects.Where(layerLayerEffect => !layerLayerEffect.PluginInfo.Enabled));
                foreach (var layerLayerEffect in disabledEffects)
                    _renderElementService.RemoveLayerEffect(layerLayerEffect);

                _renderElementService.InstantiateDisplayConditions(folder);
            }
        }

        /// <summary>
        ///     Instantiates all plugin-related classes on the layers of the given profile
        /// </summary>
        private void InstantiateLayers(Profile profile)
        {
            foreach (var layer in profile.GetAllLayers())
            {
                // Instantiate brush
                if (layer.LayerBrush == null)
                    _renderElementService.InstantiateLayerBrush(layer);
                // Remove brush if plugin is disabled
                else if (!layer.LayerBrush.PluginInfo.Enabled)
                    _renderElementService.DeactivateLayerBrush(layer);

                // Instantiate effects
                _renderElementService.InstantiateLayerEffects(layer);
                // Remove effects of plugins that are disabled
                var disabledEffects = new List<BaseLayerEffect>(layer.LayerEffects.Where(layerLayerEffect => !layerLayerEffect.PluginInfo.Enabled));
                foreach (var layerLayerEffect in disabledEffects)
                    _renderElementService.RemoveLayerEffect(layerLayerEffect);

                _renderElementService.InstantiateDisplayConditions(layer);
            }
        }

        /// <summary>
        ///     Populates all missing LEDs on all currently active profiles
        /// </summary>
        /// <param name="surface"></param>
        private void ActiveProfilesPopulateLeds(ArtemisSurface surface)
        {
            var profileModules = _pluginService.GetPluginsOfType<ProfileModule>();
            foreach (var profileModule in profileModules.Where(p => p.ActiveProfile != null).ToList())
                profileModule.ActiveProfile.PopulateLeds(surface);
        }


        /// <summary>
        ///     Instantiates all missing plugin-related classes on the profile trees of all currently active profiles
        /// </summary>
        private void ActiveProfilesInstantiatePlugins()
        {
            var profileModules = _pluginService.GetPluginsOfType<ProfileModule>();
            foreach (var profileModule in profileModules.Where(p => p.ActiveProfile != null).ToList())
            {
                InstantiateLayers(profileModule.ActiveProfile);
                InstantiateFolders(profileModule.ActiveProfile);
            }
        }

        #region Event handlers

        private void OnActiveSurfaceConfigurationSelected(object sender, SurfaceConfigurationEventArgs e)
        {
            ActiveProfilesPopulateLeds(e.Surface);
        }

        private void OnSurfaceConfigurationUpdated(object sender, SurfaceConfigurationEventArgs e)
        {
            if (e.Surface.IsActive)
                ActiveProfilesPopulateLeds(e.Surface);
        }

        private void OnPluginToggled(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is LayerBrushProvider)
                ActiveProfilesInstantiatePlugins();
            if (e.PluginInfo.Instance is LayerEffectProvider)
                ActiveProfilesInstantiatePlugins();
        }

        #endregion
    }
}