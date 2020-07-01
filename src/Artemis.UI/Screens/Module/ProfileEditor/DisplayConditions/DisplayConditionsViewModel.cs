using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionsViewModel : ProfileEditorPanelViewModel
    {
        private DisplayConditionGroupViewModel _rootGroup;

        public DisplayConditionGroupViewModel RootGroup
        {
            get => _rootGroup;
            set => SetAndNotify(ref _rootGroup, value);
        }

        public DisplayConditionsViewModel()
        {
            RootGroup = new DisplayConditionGroupViewModel {IsRootGroup = true};
            var subGroup = new DisplayConditionGroupViewModel();

            RootGroup.Children.Add(new DisplayConditionPredicateViewModel());
            RootGroup.Children.Add(new DisplayConditionPredicateViewModel());
            RootGroup.Children.Add(subGroup);
            subGroup.Children.Add(new DisplayConditionPredicateViewModel());
            subGroup.Children.Add(new DisplayConditionPredicateViewModel());
        }
    }
}