using System.Collections.Generic;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories;

public class QueuedActionRepository : IQueuedActionRepository
{
    private readonly LiteRepository _repository;

    public QueuedActionRepository(LiteRepository repository)
    {
        _repository = repository;
        _repository.Database.GetCollection<QueuedActionEntity>().EnsureIndex(s => s.Type);
    }

    #region Implementation of IQueuedActionRepository

    /// <inheritdoc />
    public void Add(QueuedActionEntity queuedActionEntity)
    {
        _repository.Insert(queuedActionEntity);
    }

    /// <inheritdoc />
    public void Remove(QueuedActionEntity queuedActionEntity)
    {
        _repository.Delete<QueuedActionEntity>(queuedActionEntity.Id);
    }

    /// <inheritdoc />
    public List<QueuedActionEntity> GetAll()
    {
        return _repository.Query<QueuedActionEntity>().ToList();
    }

    /// <inheritdoc />
    public List<QueuedActionEntity> GetByType(string type)
    {
        return _repository.Query<QueuedActionEntity>().Where(q => q.Type == type).ToList();
    }

    #endregion
}