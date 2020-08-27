using System;
using System.Linq;
using System.Reflection;
using Artemis.Core.Plugins.DataModelExpansions;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    public class DataModelListPropertiesViewModel : DataModelPropertiesViewModel
    {
        private object _displayValue;
        private int _index;
        private Type _listType;

        public DataModelListPropertiesViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, PropertyInfo propertyInfo) : base(dataModel, parent, propertyInfo)
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

        public object DisplayValue
        {
            get => _displayValue;
            set => SetAndNotify(ref _displayValue, value);
        }

        public override string PropertyPath => null;

        public override string DisplayPropertyPath => null;

        public override void Update(IDataModelUIService dataModelUIService)
        {
            // Display value gets updated by parent, don't do anything if it is null
            if (DisplayValue == null)
                return;

            ListType = DisplayValue.GetType();
            PopulateProperties(dataModelUIService);
            foreach (var dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update(dataModelUIService);
        }

        public override object GetCurrentValue()
        {
            return DisplayValue;
        }

        private void PopulateProperties(IDataModelUIService dataModelUIService)
        {
            if (Children.Any())
                return;

            foreach (var propertyInfo in ListType.GetProperties())
            {
                var child = CreateChild(dataModelUIService, propertyInfo, GetChildDepth());
                if (child != null)
                    Children.Add(child);
            }
        }
    }
}