using Artemis.Core.Models.Profile.Conditions;
using Artemis.Core.Models.Profile.Conditions.Abstract;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.DisplayConditions.Abstract
{
    public abstract class DisplayConditionViewModel : PropertyChangedBase
    {
        protected DisplayConditionViewModel(DisplayConditionPart model, DisplayConditionViewModel parent)
        {
            Model = model;
            Parent = parent;
            Children = new BindableCollection<DisplayConditionViewModel>();
        }

        public DisplayConditionPart Model { get; }
        public DisplayConditionViewModel Parent { get; set; }
        public BindableCollection<DisplayConditionViewModel> Children { get; }

        public abstract void Update();

        public void Delete()
        {
            Model.Parent.RemoveChild(Model);
            Parent.Update();
        }
    }
}