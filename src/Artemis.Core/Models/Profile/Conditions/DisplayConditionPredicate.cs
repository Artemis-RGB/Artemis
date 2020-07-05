using System;
using Artemis.Core.Models.Profile.Conditions.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionPredicate : DisplayConditionPart
    {
        public Guid DataModelGuid { get; set; }
        public string PropertyPath { get; set; }

        public DisplayConditionOperator Operator { get; set; }
        public object Value { get; set; }
    }
}