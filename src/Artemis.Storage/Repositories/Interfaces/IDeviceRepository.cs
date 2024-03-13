using System.Collections.Generic;
using Artemis.Storage.Entities.Surface;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IDeviceRepository : IRepository
{
    void Add(DeviceEntity deviceEntity);
    void Remove(DeviceEntity deviceEntity);
    DeviceEntity? Get(string id);
    List<DeviceEntity> GetAll();
    void Save(DeviceEntity deviceEntity);
    void SaveRange(IEnumerable<DeviceEntity> deviceEntities);
}