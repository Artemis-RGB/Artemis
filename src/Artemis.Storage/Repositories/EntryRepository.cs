using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Workshop;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Storage.Repositories;

internal class EntryRepository(Func<ArtemisDbContext> getContext) : IEntryRepository
{
    public void Add(EntryEntity entryEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.Entries.Add(entryEntity);
        dbContext.SaveChanges();
    }

    public void Remove(EntryEntity entryEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.Entries.Remove(entryEntity);
        dbContext.SaveChanges();
    }

    public EntryEntity? Get(Guid id)
    {
        using ArtemisDbContext dbContext = getContext();
        return dbContext.Entries.FirstOrDefault(s => s.Id == id);
    }

    public EntryEntity? GetByEntryId(long entryId)
    {
        using ArtemisDbContext dbContext = getContext();
        return dbContext.Entries.FirstOrDefault(s => s.EntryId == entryId);
    }

    public IEnumerable<EntryEntity> GetAll()
    {
        using ArtemisDbContext dbContext = getContext();
        return dbContext.Entries;
    }
    
    public void Save(EntryEntity entryEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.Update(entryEntity);
        dbContext.SaveChanges();
    }
}