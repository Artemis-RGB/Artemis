using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage.Interfaces;
using Artemis.Storage.Repositories;
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

        internal ProfileService(IPluginService pluginService, IProfileRepository profileRepository)
        {
            _pluginService = pluginService;
            _profileRepository = profileRepository;
        }

        public List<Profile> GetProfiles(ProfileModule module)
        {
            var profileEntities = _profileRepository.GetByPluginGuid(module.PluginInfo.Guid);
            var profiles = new List<Profile>();
            foreach (var profileEntity in profileEntities)
                profiles.Add(new Profile(module.PluginInfo, profileEntity, _pluginService));

            return profiles;
        }

        public Profile GetActiveProfile(ProfileModule module)
        {
            var profileEntity = _profileRepository.GetByPluginGuid(module.PluginInfo.Guid).FirstOrDefault(p => p.IsActive);
            if (profileEntity == null)
                return null;

            return new Profile(module.PluginInfo, profileEntity, _pluginService);
        }

        public Profile CreateProfile(ProfileModule module, string name)
        {
            var profile = new Profile(module.PluginInfo, name);
            
            return profile;
        }

        public void UpdateProfile(Profile profile, bool includeChildren)
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