using System;
using System.Linq;
using System.Reflection;
using Artemis.Core.Plugins.DataModelExpansions;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    public class DataModelListPropertyViewModel : DataModelPropertyViewModel
    {
        private int _index;
        private Type _listType;

        public DataModelListPropertyViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, PropertyInfo propertyInfo) : base(dataModel, parent, propertyInfo)
        {
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

        public override object GetCurrentValue()
        {
            return DisplayValue;
        }

        public override void Update(IDataModelUIService dataModelUIService)
        {
            // Display value gets updated by parent, don't do anything if it is null
            if (DisplayValue == null)
                return;

            if (DisplayViewModel == null && dataModelUIService.RegisteredDataModelDisplays.Any(d => d.SupportedType == DisplayValue.GetType()))
                dataModelUIService.GetDataModelDisplayViewModel(DisplayValue.GetType());

            ListType = DisplayValue.GetType();
            UpdateDisplayParameters();
        }
    }
}