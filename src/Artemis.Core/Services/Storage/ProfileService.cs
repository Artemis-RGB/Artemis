using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerElement;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.Services.Storage
{
    /// <summary>
    ///     Provides access to profile storage
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IPluginService _pluginService;
        private readonly IProfileRepository _profileRepository;
        private readonly ISurfaceService _surfaceService;
        private readonly ILayerService _layerService;

        internal ProfileService(IPluginService pluginService, ISurfaceService surfaceService, ILayerService layerService, IProfileRepository profileRepository)
        {
            _pluginService = pluginService;
            _surfaceService = surfaceService;
            _layerService = layerService;
            _profileRepository = profileRepository;

            _surfaceService.ActiveSurfaceConfigurationChanged += OnActiveSurfaceConfigurationChanged;
            _surfaceService.SurfaceConfigurationUpdated += OnSurfaceConfigurationUpdated;
            _pluginService.PluginLoaded += OnPluginLoaded;
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
            InstantiateProfileLayerElements(profile);
        }

        public void DeleteProfile(Profile profile)
        {
            _profileRepository.Remove(profile.ProfileEntity);
        }

        public void UpdateProfile(Profile profile, bool includeChildren)
        {
            profile.ApplyToEntity();
            if (includeChildren)
            {
                foreach (var folder in profile.GetAllFolders())
                    folder.ApplyToEntity();
                foreach (var layer in profile.GetAllLayers())
                    layer.ApplyToEntity();

                if (_surfaceService.ActiveSurface != null)
                    profile.PopulateLeds(_surfaceService.ActiveSurface);
            }

            _profileRepository.Save(profile.ProfileEntity);
        }
        private void InstantiateProfileLayerElements(Profile profile)
        {
            var layerElementProviders = _pluginService.GetPluginsOfType<LayerElementProvider>();
            var descriptors = layerElementProviders.SelectMany(l => l.LayerElementDescriptors).ToList();

            foreach (var layer in profile.GetAllLayers())
            {
                foreach (var elementEntity in layer.LayerEntity.Elements)
                {
                    // Skip already instantiated layer elements
                    if (layer.LayerElements.Any(e => e.Guid == elementEntity.Id))
                        continue;

                    // Get a matching descriptor
                    var descriptor = descriptors.FirstOrDefault(d => d.LayerElementProvider.PluginInfo.Guid == elementEntity.PluginGuid &&
                                                                     d.LayerElementType.Name == elementEntity.LayerElementType);

                    // If a descriptor that matches if found, instantiate it with the GUID of the element entity
                    if (descriptor != null)
                        _layerService.InstantiateLayerElement(layer, descriptor, elementEntity.Configuration, elementEntity.Id);
                }
            }
        }

        private void ActiveProfilesPopulateLeds(ArtemisSurface surface)
        {
            var profileModules = _pluginService.GetPluginsOfType<ProfileModule>();
            foreach (var profileModule in profileModules.Where(p => p.ActiveProfile != null).ToList())
                profileModule.ActiveProfile.PopulateLeds(surface);
        }

        private void ActiveProfilesInstantiateProfileLayerElements()
        {
            var profileModules = _pluginService.GetPluginsOfType<ProfileModule>();
            foreach (var profileModule in profileModules.Where(p => p.ActiveProfile != null).ToList())
                InstantiateProfileLayerElements(profileModule.ActiveProfile);
        }

        #region Event handlers

        private void OnActiveSurfaceConfigurationChanged(object sender, SurfaceConfigurationEventArgs e)
        {
            ActiveProfilesPopulateLeds(e.Surface);
        }

        private void OnSurfaceConfigurationUpdated(object sender, SurfaceConfigurationEventArgs e)
        {
            if (e.Surface.IsActive)
                ActiveProfilesPopulateLeds(e.Surface);
        }

        private void OnPluginLoaded(object sender, PluginEventArgs e)
        {
            if (e.PluginInfo.Instance is LayerElementProvider)
                ActiveProfilesInstantiateProfileLayerElements();
        }

        #endregion
    }
}