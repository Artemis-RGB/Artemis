using System;
using Artemis.Core.Models.Profile.Conditions.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class LayerConditionStatement : LayerConditionPart
    {
        public Guid DataModelGuid { get; set; }
        public string PropertyPath { get; set; }

        public LayerConditionOperator Operator { get; set; }
        public object Value { get; set; }
    }
}