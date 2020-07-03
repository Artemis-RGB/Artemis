using Artemis.Core.Models.Profile.Conditions.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class LayerConditionGroup : LayerConditionPart
    {
        public BooleanOperator BooleanOperator { get; set; }
    }

    public enum BooleanOperator
    {
        And,
        Or,
        AndNot,
        OrNot
    }
}