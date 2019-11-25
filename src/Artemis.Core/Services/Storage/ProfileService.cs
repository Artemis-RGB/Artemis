using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Surface;
using Artemis.Core.Plugins.Abstract;
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
        private readonly ISurfaceService _surfaceService;
        private readonly IProfileRepository _profileRepository;

        internal ProfileService(IPluginService pluginService, ISurfaceService surfaceService, IProfileRepository profileRepository)
        {
            _pluginService = pluginService;
            _surfaceService = surfaceService;
            _profileRepository = profileRepository;

            _surfaceService.ActiveSurfaceConfigurationChanged += SurfaceServiceOnActiveSurfaceConfigurationChanged;
            _surfaceService.SurfaceConfigurationUpdated += SurfaceServiceOnSurfaceConfigurationUpdated;
        }

        private void SurfaceServiceOnActiveSurfaceConfigurationChanged(object sender, SurfaceConfigurationEventArgs e)
        {
            ApplySurfaceToProfiles(e.Surface);
        }

        private void SurfaceServiceOnSurfaceConfigurationUpdated(object sender, SurfaceConfigurationEventArgs e)
        {
            if (!e.Surface.IsActive)
                return;
            ApplySurfaceToProfiles(e.Surface);
        }

        private void ApplySurfaceToProfiles(Surface surface)
        {
            var profileModules = _pluginService.GetPluginsOfType<ProfileModule>();
            foreach (var profileModule in profileModules.Where(p => p.ActiveProfile != null).ToList())
                profileModule.ActiveProfile.ApplySurface(surface);
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
                    profiles.Add(new Profile(module.PluginInfo, profileEntity, _pluginService));
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

            return new Profile(module.PluginInfo, profileEntity, _pluginService);
        }

        public Profile CreateProfile(ProfileModule module, string name)
        {
            var profile = new Profile(module.PluginInfo, name);
            _profileRepository.Add(profile.ProfileEntity);

            if (_surfaceService.ActiveSurface != null)
                profile.ApplySurface(_surfaceService.ActiveSurface);
            return profile;
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
                    profile.ApplySurface(_surfaceService.ActiveSurface);
            }

            _profileRepository.Save(profile.ProfileEntity);
        }
    }
}