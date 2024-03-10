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
    IEnumerable<EntryEntity> GetAll();
    void Save(EntryEntity entryEntity);
}