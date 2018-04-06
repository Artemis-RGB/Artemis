using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Models;
using Artemis.Core.ProfileElements;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Core.Services
{
    public class StorageService : IStorageService
    {
        private readonly StorageContext _dbContext;
        private readonly IPluginService _pluginService;

        public StorageService(StorageContext dbContext, IPluginService pluginService)
        {
            _dbContext = dbContext;
            _pluginService = pluginService;
        }

        public async Task<ICollection<Profile>> GetModuleProfiles(PluginInfo pluginInfo)
        {
            var profileEntities = await _dbContext.Profiles.Where(p => p.PluginGuid == pluginInfo.Guid).ToListAsync();
            var profiles = new List<Profile>();
            foreach (var profileEntity in profileEntities)
                profiles.Add(Profile.FromProfileEntity(pluginInfo, profileEntity, _pluginService));

            return profiles;
        }

        public async Task SaveProfile(Profile profile)
        {
            // Find a matching profile entity to update

            // If not found, create a new one

            await _dbContext.SaveChangesAsync();
        }
    }

    public interface IStorageService
    {
        Task<ICollection<Profile>> GetModuleProfiles(PluginInfo pluginInfo);
    }
}