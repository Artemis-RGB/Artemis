using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract
{
    public abstract class DisplayConditionViewModel : PropertyChangedBase
    {
        protected DisplayConditionViewModel()
        {
            Children = new BindableCollection<DisplayConditionViewModel>();
        }

        public BindableCollection<DisplayConditionViewModel> Children { get; }

        public abstract void Update();
    }
}