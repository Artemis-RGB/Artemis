using System;

namespace Artemis.Storage.Entities.Profile
{
    public class DisplayConditionPredicateEntity
    {
        public Guid LeftDataModelGuid { get; set; }
        public string LeftPropertyPath { get; set; }

        public Guid RightDataModelGuid { get; set; }
        public string RightPropertyPath { get; set; }

        // Stored as a string to be able to control serialization and deserialization ourselves
        public string RightStaticValue { get; set; }
    }
}