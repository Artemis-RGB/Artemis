using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Conditions.Abstract
{
    public abstract class DataModelConditionViewModel : Conductor<DataModelConditionViewModel>.Collection.AllActive
    {
        protected DataModelConditionViewModel(DataModelConditionPart model)
        {
            Model = model;
        }

        public DataModelConditionPart Model { get; }

        public abstract void Update();

        public virtual void Delete()
        {
            Model.Parent.RemoveChild(Model);
            ((DataModelConditionViewModel) Parent).Update();
        }
    }
}