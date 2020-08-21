using Artemis.Core.Plugins.DataModelExpansions.Attributes;
using Artemis.UI.Shared.DataModelVisualization;

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