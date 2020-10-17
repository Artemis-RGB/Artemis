using System;

namespace Artemis.UI.Shared.DefaultTypes.DataModel.Display
{
    public class DefaultDataModelDisplayViewModel : DataModelDisplayViewModel<object>
    {
        private bool _showNull;
        private bool _showToString;

        public DefaultDataModelDisplayViewModel()
        {
            ShowNull = true;
        }

        public bool ShowToString
        {
            get => _showToString;
            set => SetAndNotify(ref _showToString, value);
        }

        public bool ShowNull
        {
            get => _showNull;
            set => SetAndNotify(ref _showNull, value);
        }

        protected override void OnDisplayValueUpdated()
        {
            if (DisplayValue is Enum enumDisplayValue)
                DisplayValue = EnumUtilities.HumanizeValue(enumDisplayValue);

            ShowToString = DisplayValue != null;
            ShowNull = DisplayValue == null;
        }
    }
}