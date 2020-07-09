using Artemis.Core.Models.Profile.Conditions.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionGroup : DisplayConditionPart
    {
        public BooleanOperator BooleanOperator { get; set; }

        public override void ApplyToEntity()
        {
            foreach (var child in Children)
                child.ApplyToEntity();
        }
    }

    public enum BooleanOperator
    {
        And,
        Or,
        AndNot,
        OrNot
    }
}