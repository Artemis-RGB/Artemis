using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Workshop;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Storage.Repositories;

internal class EntryRepository : IEntryRepository
{
    private readonly ArtemisDbContext _dbContext;

    public EntryRepository(ArtemisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(EntryEntity entryEntity)
    {
        _dbContext.Entries.Add(entryEntity);
        SaveChanges();
    }

    public void Remove(EntryEntity entryEntity)
    {
        _dbContext.Entries.Remove(entryEntity);
        SaveChanges();
    }

    public EntryEntity? Get(Guid id)
    {
        return _dbContext.Entries.FirstOrDefault(s => s.Id == id);
    }

    public EntryEntity? GetByEntryId(long entryId)
    {
        return _dbContext.Entries.FirstOrDefault(s => s.EntryId == entryId);
    }

    public IEnumerable<EntryEntity> GetAll()
    {
        return _dbContext.Entries;
    }

    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }
}