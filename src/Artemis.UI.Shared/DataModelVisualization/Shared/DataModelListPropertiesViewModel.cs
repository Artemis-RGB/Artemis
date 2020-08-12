using System;
using System.Linq;
using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.UI.Shared.Services;

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

        public override string PropertyPath => Parent?.PropertyPath;

        public override string DisplayPropertyPath => Parent?.DisplayPropertyPath;

        public override void Update(IDataModelVisualizationService dataModelVisualizationService)
        {
            // Display value gets updated by parent, don't do anything if it is null
            if (DisplayValue == null)
                return;

            ListType = DisplayValue.GetType();
            PopulateProperties(dataModelVisualizationService);
            foreach (var dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update(dataModelVisualizationService);
        }

        public override object GetCurrentValue()
        {
            return DisplayValue;
        }

        private void PopulateProperties(IDataModelVisualizationService dataModelVisualizationService)
        {
            if (Children.Any())
                return;

            foreach (var propertyInfo in ListType.GetProperties())
            {
                var child = CreateChild(dataModelVisualizationService, propertyInfo, GetChildDepth());
                if (child != null)
                    Children.Add(child);
            }
        }
    }
}