using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions.Abstract
{
    public abstract class DisplayConditionViewModel : Conductor<DisplayConditionViewModel>.Collection.AllActive
    {
        protected DisplayConditionViewModel(DataModelConditionPart model)
        {
            Model = model;
        }

        public DataModelConditionPart Model { get; }

        public abstract void Update();

        public virtual void Delete()
        {
            Model.Parent.RemoveChild(Model);
            ((DisplayConditionViewModel) Parent).Update();
        }
    }
}