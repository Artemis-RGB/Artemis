using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.General
{
    public class QueuedActionEntity
    {
        public QueuedActionEntity()
        {
            Parameters = new Dictionary<string, object>();
        }

        public Guid Id { get; set; }
        public string Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public Dictionary<string, object> Parameters { get; set; }
    }
}