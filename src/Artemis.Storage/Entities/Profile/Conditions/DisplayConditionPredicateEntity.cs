using System;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Storage.Entities.Profile.Conditions
{
    public class DisplayConditionPredicateEntity : DisplayConditionPartEntity
    {
        public int PredicateType { get; set; }
        public Guid? LeftDataModelGuid { get; set; }
        public string LeftPropertyPath { get; set; }

        public Guid? RightDataModelGuid { get; set; }
        public string RightPropertyPath { get; set; }

        public string OperatorType { get; set; }
        public Guid? OperatorPluginGuid { get; set; }

        // Stored as a string to be able to control serialization and deserialization ourselves
        public string RightStaticValue { get; set; }
    }
}