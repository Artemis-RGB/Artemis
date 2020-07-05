using Artemis.Core.Models.Profile.Conditions;
using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionListPredicateViewModel : DisplayConditionViewModel
    {
        public DisplayConditionListPredicateViewModel(DisplayConditionListPredicate displayConditionListPredicate, DisplayConditionViewModel parent) : base(displayConditionListPredicate, parent)
        {
        }

        public DisplayConditionListPredicate DisplayConditionListPredicate => (DisplayConditionListPredicate) Model;

        public override void Update()
        {
        }
    }
}