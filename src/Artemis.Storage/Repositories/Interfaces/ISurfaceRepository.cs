using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Storage.Entities;

namespace Artemis.Storage.Repositories.Interfaces
{
    public interface ISurfaceRepository : IRepository
    {
        void Add(SurfaceEntity surfaceEntity);
        SurfaceEntity Get(string name);
        Task<SurfaceEntity> GetAsync(string name);
        List<SurfaceEntity> GetAll();
        Task<List<SurfaceEntity>> GetAllAsync();

        void Save();
        Task SaveAsync();
    }
}