using Artemis.Core.Models.Profile.Conditions.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class LayerConditionListStatement : LayerConditionPart
    {
        public ListOperator ListOperator { get; set; }

    }

    public enum ListOperator
    {
        Any,
        All,
        None,
        Count
    }
}