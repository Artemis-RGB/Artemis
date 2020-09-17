using System;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared;
using Stylet;

namespace Artemis.UI.DefaultTypes.DataModel.Input
{
    public class EnumDataModelInputViewModel : DataModelInputViewModel<Enum>
    {
        public EnumDataModelInputViewModel(DataModelPropertyAttribute targetDescription, Enum initialValue) : base(targetDescription, initialValue)
        {
            EnumValues = new BindableCollection<ValueDescription>(EnumUtilities.GetAllValuesAndDescriptions(initialValue.GetType()));
        }

        public BindableCollection<ValueDescription> EnumValues { get; set; }
    }
}