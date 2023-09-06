using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Workshop;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories;

internal class EntryRepository : IEntryRepository
{
    private readonly LiteRepository _repository;

    public EntryRepository(LiteRepository repository)
    {
        _repository = repository;
        _repository.Database.GetCollection<EntryEntity>().EnsureIndex(s => s.Id);
        _repository.Database.GetCollection<EntryEntity>().EnsureIndex(s => s.EntryId);
    }

    public void Add(EntryEntity entryEntity)
    {
        _repository.Insert(entryEntity);
    }

    public void Remove(EntryEntity entryEntity)
    {
        _repository.Delete<EntryEntity>(entryEntity.Id);
    }

    public EntryEntity Get(Guid id)
    {
        return _repository.FirstOrDefault<EntryEntity>(s => s.Id == id);
    }

    public EntryEntity GetByEntryId(Guid entryId)
    {
        return _repository.FirstOrDefault<EntryEntity>(s => s.EntryId == entryId);
    }

    public List<EntryEntity> GetAll()
    {
        return _repository.Query<EntryEntity>().ToList();
    }

    public void Save(EntryEntity entryEntity)
    {
        _repository.Upsert(entryEntity);
    }

    public void Save(IEnumerable<EntryEntity> entryEntities)
    {
        _repository.Upsert(entryEntities);
    }
}