using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Models;
using Artemis.Core.ProfileElements;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Repositories;

namespace Artemis.Core.Services
{
    public class StorageService : IStorageService
    {
        private readonly IPluginService _pluginService;
        private readonly ProfileRepository _profileRepository;

        internal StorageService(IPluginService pluginService)
        {
            _pluginService = pluginService;
            _profileRepository = new ProfileRepository();
        }

        public async Task<ICollection<Profile>> GetModuleProfiles(PluginInfo pluginInfo)
        {
            var profileEntities = await _profileRepository.GetByPluginGuidAsync(pluginInfo.Guid);
            var profiles = new List<Profile>();
            foreach (var profileEntity in profileEntities)
                profiles.Add(Profile.FromProfileEntity(pluginInfo, profileEntity, _pluginService));

            return profiles;
        }

        public async Task SaveProfile(Profile profile)
        {
            // Find a matching profile entity to update

            // If not found, create a new one

            await _profileRepository.SaveAsync();
        }
    }

    public interface IStorageService
    {
        Task<ICollection<Profile>> GetModuleProfiles(PluginInfo pluginInfo);
    }
}