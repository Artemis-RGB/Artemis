using Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions
{
    public class DisplayConditionGroupViewModel : DisplayConditionViewModel
    {
        private bool _isRootGroup;

        public bool IsRootGroup
        {
            get => _isRootGroup;
            set => SetAndNotify(ref _isRootGroup, value);
        }

        public DisplayConditionGroupViewModel()
        {
        }

        public override void Update()
        {
            
        }
    }
}