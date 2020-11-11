using System;

namespace Artemis.Storage.Entities.Profile
{
    public class DataModelPathEntity
    {
        public string Path { get; set; }
        public string DataModelId { get; set; }

        public PathWrapperType WrapperType { get; set; }
    }

    public enum PathWrapperType
    {
        None,
        List,
        Event
    }
}