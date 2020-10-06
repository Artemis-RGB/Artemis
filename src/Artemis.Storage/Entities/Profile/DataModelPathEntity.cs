using System;

namespace Artemis.Storage.Entities.Profile
{
    public class DataModelPathEntity
    {
        public string Path { get; set; }
        public Guid? DataModelGuid { get; set; }
    }
}