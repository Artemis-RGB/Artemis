using System;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public abstract class DataModelConditionPredicateEntity : DataModelConditionPartEntity
    {
        public int PredicateType { get; set; }
        public DataModelPathEntity LeftPath { get; set; }
        public DataModelPathEntity RightPath { get; set; }

        public string OperatorType { get; set; }
        public Guid? OperatorPluginGuid { get; set; }

        // Stored as a string to be able to control serialization and deserialization ourselves
        public string RightStaticValue { get; set; }
    }
}