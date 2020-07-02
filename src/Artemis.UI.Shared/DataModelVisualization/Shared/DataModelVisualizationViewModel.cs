using System;
using System.Collections;
using System.Reflection;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Artemis.UI.Shared.Services;
using Humanizer;
using Stylet;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    public abstract class DataModelVisualizationViewModel : PropertyChangedBase
    {
        private bool _isListProperty;
        private string _listDescription;
        private object _model;
        private DataModelVisualizationViewModel _parent;
        private DataModelPropertyAttribute _propertyDescription;
        private PropertyInfo _propertyInfo;
        private Type _propertyType;

        internal DataModelVisualizationViewModel()
        {
        }

        public DataModelPropertyAttribute PropertyDescription
        {
            get => _propertyDescription;
            protected set => SetAndNotify(ref _propertyDescription, value);
        }

        public PropertyInfo PropertyInfo
        {
            get => _propertyInfo;
            protected set => SetAndNotify(ref _propertyInfo, value);
        }

        public Type PropertyType
        {
            get => _propertyType;
            set => SetAndNotify(ref _propertyType, value);
        }

        public DataModelVisualizationViewModel Parent
        {
            get => _parent;
            protected set => SetAndNotify(ref _parent, value);
        }

        public object Model
        {
            get => _model;
            set => SetAndNotify(ref _model, value);
        }

        public bool IsListProperty
        {
            get => _isListProperty;
            set => SetAndNotify(ref _isListProperty, value);
        }

        public string ListDescription
        {
            get => _listDescription;
            set => SetAndNotify(ref _listDescription, value);
        }

        public abstract void Update();

        protected DataModelVisualizationViewModel CreateChild(IDataModelVisualizationService dataModelVisualizationService, PropertyInfo propertyInfo)
        {
            // Skip properties decorated with DataModelIgnore
            if (Attribute.IsDefined(propertyInfo, typeof(DataModelIgnoreAttribute)))
                return null;

            var dataModelPropertyAttribute = (DataModelPropertyAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute));
            // If no DataModelProperty attribute was provided, pull one out of our ass
            if (dataModelPropertyAttribute == null)
                dataModelPropertyAttribute = new DataModelPropertyAttribute {Name = propertyInfo.Name.Humanize()};

            // If a display VM was found, prefer to use that in any case
            var typeViewModel = dataModelVisualizationService.GetDataModelDisplayViewModel(propertyInfo.PropertyType);
            if (typeViewModel != null)
                return new DataModelPropertyViewModel(propertyInfo, dataModelPropertyAttribute, this) {DisplayViewModel = typeViewModel};
            // For primitives, create a property view model, it may be null that is fine
            if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string))
                return new DataModelPropertyViewModel(propertyInfo, dataModelPropertyAttribute, this);
            if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
                return new DataModelListViewModel(propertyInfo, dataModelPropertyAttribute, this, dataModelVisualizationService);
            // For other value types create a child view model if the value type is not null
            if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsStruct())
            {
                var value = propertyInfo.GetValue(Model);
                if (value == null)
                    return null;

                return new DataModelViewModel(propertyInfo, value, dataModelPropertyAttribute, this, dataModelVisualizationService);
            }

            return null;
        }

        protected DataModelVisualizationViewModel CreateChild(IDataModelVisualizationService dataModelVisualizationService, object value)
        {
            var dataModelPropertyAttribute = new DataModelPropertyAttribute {Name = "Unknown property"};

            // If a display VM was found, prefer to use that in any case
            var typeViewModel = dataModelVisualizationService.GetDataModelDisplayViewModel(value.GetType());
            if (typeViewModel != null)
                return new DataModelPropertyViewModel(null, dataModelPropertyAttribute, this) {Model = value, DisplayViewModel = typeViewModel};
            // For primitives, create a property view model, it may be null that is fine
            if (value.GetType().IsPrimitive || value is string)
                return new DataModelPropertyViewModel(null, dataModelPropertyAttribute, this) {Model = value};
            // For other value types create a child view model if the value type is not null
            if (value.GetType().IsClass || value.GetType().IsStruct())
                return new DataModelViewModel(null, value, dataModelPropertyAttribute, this, dataModelVisualizationService);

            return null;
        }

        protected void UpdateListStatus()
        {
            if (Parent is DataModelListViewModel listViewModel)
            {
                IsListProperty = true;
                ListDescription = $"List item [{listViewModel.List.IndexOf(Model)}]";
                PropertyType = Model.GetType();
            }
            else
            {
                IsListProperty = false;
                PropertyType = PropertyInfo?.PropertyType;
            }
        }
    }
}