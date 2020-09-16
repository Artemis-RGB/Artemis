using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared;
using SkiaSharp;

namespace Artemis.UI.DefaultTypes.DataModel.Input
{
    public class SKColorDataModelInputViewModel : DataModelInputViewModel<SKColor>
    {
        public SKColorDataModelInputViewModel(DataModelPropertyAttribute targetDescription, SKColor initialValue) : base(targetDescription, initialValue)
        {
        }
    }
}