using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions.Abstract
{
    public abstract class DisplayConditionViewModel : Conductor<DisplayConditionViewModel>.Collection.AllActive
    {
        protected DisplayConditionViewModel(DisplayConditionPart model)
        {
            Model = model;
        }

        public DisplayConditionPart Model { get; }

        public abstract void Update();

        public virtual void Delete()
        {
            Model.Parent.RemoveChild(Model);
            ((DisplayConditionViewModel) Parent).Update();
        }
    }
}