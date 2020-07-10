using Artemis.Core.Models.Profile.Conditions.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core.Models.Profile.Conditions
{
    public class DisplayConditionListPredicate : DisplayConditionPart
    {
        public ListOperator ListOperator { get; set; }

        internal override void ApplyToEntity()
        {
        }

        internal override void Initialize(IDataModelService dataModelService)
        {
        }

        public override DisplayConditionPartEntity GetEntity()
        {
            return null;
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