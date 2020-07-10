using Artemis.Core.Models.Profile.Conditions.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionListPredicate : DisplayConditionPart
    {
        public ListOperator ListOperator { get; set; }

        public override void ApplyToEntity()
        {
            
        }
    }

    public enum ListOperator
    {
        Any,
        All,
        None,
        Count
    }
}