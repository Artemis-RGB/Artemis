using System;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelListPropertiesViewModel : DataModelPropertiesViewModel
    {
        private object _displayValue;
        private int _index;
        private Type _listType;

        public DataModelListPropertiesViewModel(Type listType) : base(null, null, null)
        {
            DataModel = ListPredicateWrapperDataModel.Create(listType);
            ListType = listType;

            IsRootViewModel = false;
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

        public DataModelVisualizationViewModel DisplayViewModel => Children.FirstOrDefault();

        public override string DisplayPath => null;

        public override void Update(IDataModelUIService dataModelUIService)
        {
            ((ListPredicateWrapperDataModel) DataModel).UntypedValue = DisplayValue;

            PopulateProperties(dataModelUIService);
            if (DisplayViewModel == null)
                return;

            if (IsVisualizationExpanded && !DisplayViewModel.IsVisualizationExpanded)
                DisplayViewModel.IsVisualizationExpanded = IsVisualizationExpanded;
            DisplayViewModel.Update(dataModelUIService);
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