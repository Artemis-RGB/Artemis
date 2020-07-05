using System;
using System.Linq;
using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.UI.Shared.Services;

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

        public override void Update(IDataModelVisualizationService dataModelVisualizationService)
        {
            // Display value gets updated by parent, don't do anything if it is null
            if (DisplayValue == null)
                return;

            if (DisplayViewModel == null && dataModelVisualizationService.RegisteredDataModelDisplays.Any(d => d.SupportedType == DisplayValue.GetType()))
                dataModelVisualizationService.GetDataModelDisplayViewModel(DisplayValue.GetType());

            ListType = DisplayValue.GetType();
            UpdateDisplayParameters();
        }
    }
}