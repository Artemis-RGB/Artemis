using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Workshop;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IEntryRepository : IRepository
{
    void Add(EntryEntity entryEntity);
    void Remove(EntryEntity entryEntity);
    EntryEntity? Get(Guid id);
    EntryEntity? GetByEntryId(long entryId);
    List<EntryEntity> GetAll();
    void SaveChanges();
}