using Artemis.Core.Modules;
using Artemis.UI.Shared;

namespace Artemis.UI.DefaultTypes.DataModel.Input
{
    public class StringDataModelInputViewModel : DataModelInputViewModel<string>
    {
        public StringDataModelInputViewModel(DataModelPropertyAttribute targetDescription, string initialValue) : base(targetDescription, initialValue)
        {
        }

        protected override void OnSubmit()
        {
            if (string.IsNullOrWhiteSpace(InputValue))
                InputValue = null;
        }
    }
}