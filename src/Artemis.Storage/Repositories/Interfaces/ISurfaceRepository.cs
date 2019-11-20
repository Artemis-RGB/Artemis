using System.Collections.Generic;
using Artemis.Storage.Entities;
using Artemis.Storage.Entities.Surface;

namespace Artemis.Storage.Repositories.Interfaces
{
    public interface ISurfaceRepository : IRepository
    {
        void Add(SurfaceEntity surfaceEntity);
        void Remove(SurfaceEntity surfaceEntity);
        SurfaceEntity GetByName(string name);
        List<SurfaceEntity> GetAll();

        void Save(SurfaceEntity surfaceEntity);
    }
}