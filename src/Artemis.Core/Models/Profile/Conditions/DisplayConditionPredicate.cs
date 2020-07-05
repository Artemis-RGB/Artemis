using System;
using Artemis.Core.Models.Profile.Conditions.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionPredicate : DisplayConditionPart
    {
        public PredicateType PredicateType { get; set; }
        public DisplayConditionOperator Operator { get; set; }

        public Guid LeftDataModelGuid { get; set; }
        public string LeftPropertyPath { get; set; }
        
        public Guid RightDataModelGuid { get; set; }
        public string RightPropertyPath { get; set; }
        public object RightStaticValue { get; set; }
    }

    public enum PredicateType
    {
        Static,
        Dynamic
    }
}