using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelPropertyViewModel : DataModelVisualizationViewModel
    {
        private object _displayValue;
        private DataModelDisplayViewModel _displayViewModel;

        internal DataModelPropertyViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, PropertyInfo propertyInfo) : base(dataModel, parent, propertyInfo)
        {
        }

        public object DisplayValue
        {
            get => _displayValue;
            set => SetAndNotify(ref _displayValue, value);
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
                DisplayViewModel = dataModelUIService.GetDataModelDisplayViewModel(PropertyInfo.PropertyType, true);

            DisplayValue = GetCurrentValue();
            UpdateDisplayParameters();
        }

        protected void UpdateDisplayParameters()
        {
            DisplayViewModel?.UpdateValue(DisplayValue);
        }
    }
}