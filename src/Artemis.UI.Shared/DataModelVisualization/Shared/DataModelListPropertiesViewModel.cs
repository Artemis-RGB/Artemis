using System;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelListPropertiesViewModel : DataModelPropertiesViewModel
    {
        private object _displayValue;
        private int _index;
        private Type _listType;

        public DataModelListPropertiesViewModel(object listItem) : base(null, null, null)
        {
            DataModel = ListPredicateWrapperDataModel.Create(listItem.GetType());
            ListType = listItem.GetType();
            DisplayValue = listItem;
        }

        public int Index
        {
            get => _index;
            set => SetAndNotify(ref _index, value);
        }

        public Type ListType
        {
            get => _listType;
            set => SetAndNotify(ref _listType, value);
        }

        public object DisplayValue
        {
            get => _displayValue;
            set => SetAndNotify(ref _displayValue, value);
        }

        public override string DisplayPath => null;

        public override void Update(IDataModelUIService dataModelUIService)
        {
            // Display value gets updated by parent, don't do anything if it is null
            if (DisplayValue == null)
                return;

            ListType = DisplayValue.GetType();
            PopulateProperties(dataModelUIService);
            foreach (DataModelVisualizationViewModel dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update(dataModelUIService);
        }

        public override object GetCurrentValue()
        {
            return DisplayValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[List item {Index}] {DisplayPath ?? Path}";
        }
    }
}