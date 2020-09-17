using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared;

namespace Artemis.UI.DefaultTypes.DataModel.Input
{
    public class BoolDataModelInputViewModel : DataModelInputViewModel<bool>
    {
        public BoolDataModelInputViewModel(DataModelPropertyAttribute targetDescription, bool initialValue) : base(targetDescription, initialValue)
        {
        }
    }
}