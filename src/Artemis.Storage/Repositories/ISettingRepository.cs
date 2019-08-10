using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Storage.Entities;

namespace Artemis.Storage.Repositories
{
    public interface ISettingRepository : IRepository
    {
        void Add(SettingEntity settingEntity);
        SettingEntity Get(string name);
        Task<SettingEntity> GetAsync(string name);
        List<SettingEntity> GetAll();
        Task<List<SettingEntity>> GetAllAsync();
        void Save();
        Task SaveAsync();
    }
}