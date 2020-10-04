using System;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelPropertyViewModel : DataModelVisualizationViewModel
    {
        private object _displayValue;
        private DataModelDisplayViewModel _displayViewModel;
        private Type _displayValueType;

        internal DataModelPropertyViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath) : base(dataModel, parent, dataModelPath)
        {
        }

        public object DisplayValue
        {
            get => _displayValue;
            set => SetAndNotify(ref _displayValue, value);
        }

        public Type DisplayValueType
        {
            get => _displayValueType;
            set => SetAndNotify(ref _displayValueType, value);
        }

        public DataModelDisplayViewModel DisplayViewModel
        {
            get => _displayViewModel;
            set => SetAndNotify(ref _displayViewModel, value);
        }

        public override void Update(IDataModelUIService dataModelUIService)
        {
            if (Parent != null && !Parent.IsVisualizationExpanded && !Parent.IsRootViewModel)
                return;

            if (DisplayViewModel == null)
                DisplayViewModel = dataModelUIService.GetDataModelDisplayViewModel(DataModelPath.GetPropertyType(), true);

            DisplayValue = GetCurrentValue();
            DisplayValueType = DataModelPath.GetPropertyType();
            UpdateDisplayParameters();
        }
        
        protected void UpdateDisplayParameters()
        {
            DisplayViewModel?.UpdateValue(DisplayValue);
        }
    }
}