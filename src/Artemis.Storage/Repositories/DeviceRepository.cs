using System.Collections.Generic;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories;

internal class DeviceRepository : IDeviceRepository
{
    private readonly LiteRepository _repository;

    public DeviceRepository(LiteRepository repository)
    {
        _repository = repository;
        _repository.Database.GetCollection<DeviceEntity>().EnsureIndex(s => s.Id);
    }

    public void Add(DeviceEntity deviceEntity)
    {
        _repository.Insert(deviceEntity);
    }

    public void Remove(DeviceEntity deviceEntity)
    {
        _repository.Delete<DeviceEntity>(deviceEntity.Id);
    }

    public DeviceEntity? Get(string id)
    {
        return _repository.FirstOrDefault<DeviceEntity>(s => s.Id == id);
    }

    public List<DeviceEntity> GetAll()
    {
        return _repository.Query<DeviceEntity>().Include(s => s.InputIdentifiers).ToList();
    }

    public void Save(DeviceEntity deviceEntity)
    {
        _repository.Upsert(deviceEntity);
    }

    public void Save(IEnumerable<DeviceEntity> deviceEntities)
    {
        _repository.Upsert(deviceEntities);
    }
}