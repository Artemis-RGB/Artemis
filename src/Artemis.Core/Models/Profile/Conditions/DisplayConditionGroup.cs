using Artemis.Core.Models.Profile.Conditions.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionGroup : DisplayConditionPart
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