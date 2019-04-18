using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Repositories
{
    public class ProfileRepository
    {
        private readonly StorageContext _dbContext;

        public ProfileRepository()
        {
            _dbContext = new StorageContext();
            _dbContext.Database.EnsureCreated();
        }

        public IQueryable<ProfileEntity> GetAll()
        {
            return _dbContext.Profiles;
        }

        public async Task<IList<ProfileEntity>> GetByPluginGuidAsync(Guid pluginGuid)
        {
            return await _dbContext.Profiles.Where(p => p.PluginGuid == pluginGuid).ToListAsync();
        }

        public async Task<ProfileEntity> GetByGuidAsync(string guid)
        {
            return await _dbContext.Profiles.FirstOrDefaultAsync(p => p.Guid == guid);
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}