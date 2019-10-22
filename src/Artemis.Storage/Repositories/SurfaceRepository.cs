using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Repositories
{
    public class SurfaceRepository : ISurfaceRepository
    {
        private readonly StorageContext _dbContext;

        internal SurfaceRepository()
        {
            _dbContext = new StorageContext();
            _dbContext.Database.EnsureCreated();
        }

        public void Add(SurfaceEntity surfaceEntity)
        {
            _dbContext.Surfaces.Add(surfaceEntity);
        }

        public void Remove(SurfaceEntity surfaceEntity)
        {
            _dbContext.Surfaces.Remove(surfaceEntity);
        }

        public SurfaceEntity Get(string name)
        {
            return _dbContext.Surfaces.Include(s => s.SurfacePositions).FirstOrDefault(p => p.Name == name);
        }

        public async Task<SurfaceEntity> GetAsync(string name)
        {
            return await _dbContext.Surfaces.Include(s => s.SurfacePositions).FirstOrDefaultAsync(p => p.Name == name);
        }

        public List<SurfaceEntity> GetAll()
        {
            return _dbContext.Surfaces.Include(s => s.SurfacePositions).ToList();
        }

        public async Task<List<SurfaceEntity>> GetAllAsync()
        {
            return await _dbContext.Surfaces.Include(s => s.SurfacePositions).ToListAsync();
        }
        
        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}