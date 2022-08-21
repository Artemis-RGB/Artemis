using System.Collections.Generic;
using Artemis.Storage.Entities.General;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IQueuedActionRepository : IRepository
{
    void Add(QueuedActionEntity queuedActionEntity);
    void Remove(QueuedActionEntity queuedActionEntity);
    List<QueuedActionEntity> GetAll();
    List<QueuedActionEntity> GetByType(string type);
}