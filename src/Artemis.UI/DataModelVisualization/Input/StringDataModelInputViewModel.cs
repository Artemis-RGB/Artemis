using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared;

namespace Artemis.UI.DataModelVisualization.Input
{
    public class StringDataModelInputViewModel : DataModelInputViewModel<string>
    {
        public StringDataModelInputViewModel(DataModelPropertyAttribute description, string initialValue) : base(description, initialValue)
        {
        }

        protected override void OnSubmit()
        {
            if (string.IsNullOrWhiteSpace(InputValue))
                InputValue = null;
        }
    }
}