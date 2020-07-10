using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerEffect.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace Artemis.Core.Services.Storage
{
    /// <summary>
    ///     Provides access to profile storage and is responsible for activating default profiles
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IRenderElementService _renderElementService;
        private readonly ILogger _logger;
        private readonly IPluginService _pluginService;
        private readonly IProfileRepository _profileRepository;
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

        public void ActivateDefaultProfiles()
        {
            foreach (var profileModule in _pluginService.GetPluginsOfType<ProfileModule>())
            {
                var activeProfile = GetActiveProfile(profileModule);
                ActivateProfile(profileModule, activeProfile);
            }
        }

        public List<Profile> GetProfiles(ProfileModule module)
        {
            var profileEntities = _profileRepository.GetByPluginGuid(module.PluginInfo.Guid);
            var profiles = new List<Profile>();
            foreach (var profileEntity in profileEntities)
            {
                // If the profile entity matches the module's currently active profile, use that instead
                if (module.ActiveProfile != null && module.ActiveProfile.EntityId == profileEntity.Id)
                    profiles.Add(module.ActiveProfile);
                else
                    profiles.Add(new Profile(module.PluginInfo, profileEntity));
            }

            return profiles;
        }

        public Profile GetActiveProfile(ProfileModule module)
        {
            if (module.ActiveProfile != null)
                return module.ActiveProfile;

            var moduleProfiles = _profileRepository.GetByPluginGuid(module.PluginInfo.Guid);
            var profileEntity = moduleProfiles.FirstOrDefault(p => p.IsActive) ?? moduleProfiles.FirstOrDefault();
            if (profileEntity == null)
                return null;

            return new Profile(module.PluginInfo, profileEntity);
        }

        public Profile CreateProfile(ProfileModule module, string name)
        {
            var profile = new Profile(module.PluginInfo, name);
            _profileRepository.Add(profile.ProfileEntity);

            if (_surfaceService.ActiveSurface != null)
                profile.PopulateLeds(_surfaceService.ActiveSurface);

            return profile;
        }


        public void ActivateProfile(ProfileModule module, Profile profile)
        {
            module.ChangeActiveProfile(profile, _surfaceService.ActiveSurface);
            if (profile != null)
            {
                InitializeLayerProperties(profile);
                InstantiateLayers(profile);
                InstantiateFolders(profile);
            }
        }

        public void DeleteProfile(Profile profile)
        {
            _logger.Debug("Removing profile " + profile);
            _profileRepository.Remove(profile.ProfileEntity);
        }

        public void UpdateProfile(Profile profile, bool includeChildren)
        {
            _logger.Debug("Updating profile " + profile);
            var memento = JsonConvert.SerializeObject(profile.ProfileEntity);
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

        public bool UndoUpdateProfile(Profile profile, ProfileModule module)
        {
            if (!profile.UndoStack.Any())
            {
                _logger.Debug("Undo profile update - Failed, undo stack empty");
                return false;
            }

            ActivateProfile(module, null);
            var top = profile.UndoStack.Pop();
            var memento = JsonConvert.SerializeObject(profile.ProfileEntity);
            profile.RedoStack.Push(memento);
            profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top);
            profile.ApplyToProfile();
            ActivateProfile(module, profile);

            _logger.Debug("Undo profile update - Success");
            return true;
        }

        public bool RedoUpdateProfile(Profile profile, ProfileModule module)
        {
            if (!profile.RedoStack.Any())
            {
                _logger.Debug("Redo profile update - Failed, redo empty");
                return false;
            }

            ActivateProfile(module, null);
            var top = profile.RedoStack.Pop();
            var memento = JsonConvert.SerializeObject(profile.ProfileEntity);
            profile.UndoStack.Push(memento);
            profile.ProfileEntity = JsonConvert.DeserializeObject<ProfileEntity>(top);
            profile.ApplyToProfile();
            ActivateProfile(module, profile);

            _logger.Debug("Redo profile update - Success");
            return true;
        }

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

        private void ActiveProfilesPopulateLeds(ArtemisSurface surface)
        {
            var profileModules = _pluginService.GetPluginsOfType<ProfileModule>();
            foreach (var profileModule in profileModules.Where(p => p.ActiveProfile != null).ToList())
                profileModule.ActiveProfile.PopulateLeds(surface);
        }

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
            else if (e.PluginInfo.Instance is ProfileModule profileModule)
            {
                var activeProfile = GetActiveProfile(profileModule);
                ActivateProfile(profileModule, activeProfile);
            }
        }

        #endregion
    }
}