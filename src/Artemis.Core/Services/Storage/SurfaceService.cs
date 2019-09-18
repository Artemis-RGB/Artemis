using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.Services.Storage
{
    public class SurfaceService : ISurfaceService
    {
        private readonly ISurfaceRepository _surfaceRepository;

        public SurfaceService(ISurfaceRepository surfaceRepository)
        {
            _surfaceRepository = surfaceRepository;
        }

        public async Task<List<SurfaceConfiguration>> GetSurfaceConfigurations()
        {
            var surfaceEntities = await _surfaceRepository.GetAllAsync();
            var configs = new List<SurfaceConfiguration>();
            foreach (var surfaceEntity in surfaceEntities)
                configs.Add(new SurfaceConfiguration(surfaceEntity));

            return configs;
        }

        public SurfaceConfiguration GetActiveSurfaceConfiguration()
        {
            var entity = _surfaceRepository.GetAll().FirstOrDefault(d => d.IsActive);
            return entity != null ? new SurfaceConfiguration(entity) : null;
        }
    }

    public interface ISurfaceService : IArtemisService
    {
        Task<List<SurfaceConfiguration>> GetSurfaceConfigurations();
        SurfaceConfiguration GetActiveSurfaceConfiguration();
    }
}