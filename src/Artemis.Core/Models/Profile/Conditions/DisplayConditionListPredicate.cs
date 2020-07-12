using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionListPredicate : DisplayConditionPart
    {
        public ListOperator ListOperator { get; set; }

        public override bool Evaluate()
        {
            return true;
        }


        internal override void ApplyToEntity()
        {
        }

        internal override DisplayConditionPartEntity GetEntity()
        {
            return null;
        }

        internal override void Initialize(IDataModelService dataModelService)
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