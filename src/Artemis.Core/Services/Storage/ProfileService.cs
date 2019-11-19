using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage.Repositories;

namespace Artemis.Core.Services.Storage
{
    /// <summary>
    ///     Provides access to profile storage
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IPluginService _pluginService;
        private readonly ProfileRepository _profileRepository;

        internal ProfileService(IPluginService pluginService)
        {
            _pluginService = pluginService;
            _profileRepository = new ProfileRepository();
        }

        public async Task<ICollection<Profile>> GetProfiles(ProfileModule module)
        {
            var profileEntities = await _profileRepository.GetByPluginGuidAsync(module.PluginInfo.Guid);
            var profiles = new List<Profile>();
            foreach (var profileEntity in profileEntities)
                profiles.Add(new Profile(module.PluginInfo, profileEntity, _pluginService));

            return profiles;
        }

        public async Task<Profile> GetActiveProfile(ProfileModule module)
        {
            var profileEntity = await _profileRepository.GetActiveProfileByPluginGuidAsync(module.PluginInfo.Guid);
            if (profileEntity == null)
                return null;

            return new Profile(module.PluginInfo, profileEntity, _pluginService);
        }

        public async Task<Profile> CreateProfile(ProfileModule module, string name)
        {
            var profile = new Profile(module.PluginInfo, name);
            
            return profile;
        }

        public async Task UpdateProfile(Profile profile, bool includeChildren)
        {
            profile.ApplyToEntity();
            if (includeChildren)
            {
                foreach (var profileElement in profile.Children)
                {
                    profileElement.ApplyToEntity();
                }
            }
        }
    }
}