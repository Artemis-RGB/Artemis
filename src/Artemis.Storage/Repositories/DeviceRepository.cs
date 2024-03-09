using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Storage.Repositories;

internal class DeviceRepository : IDeviceRepository
{
    private readonly ArtemisDbContext _dbContext;

    public DeviceRepository(ArtemisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(DeviceEntity deviceEntity)
    {
        _dbContext.Devices.Add(deviceEntity);
        SaveChanges();
    }

    public void Remove(DeviceEntity deviceEntity)
    {
        _dbContext.Devices.Remove(deviceEntity);
        SaveChanges();
    }

    public DeviceEntity? Get(string id)
    {
        return _dbContext.Devices.FirstOrDefault(d => d.Id == id);
    }

    public IEnumerable<DeviceEntity> GetAll()
    {
        return _dbContext.Devices;
    }

    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }
}