using System;
using System.Collections;
using System.Reflection;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Humanizer;
using Stylet;

namespace Artemis.UI.DataModelVisualization
{
    public abstract class DataModelVisualizationViewModel : PropertyChangedBase
    {
        public PropertyInfo PropertyInfo { get; protected set; }
        public DataModelPropertyAttribute PropertyDescription { get; protected set; }
        public DataModelVisualizationViewModel Parent { get; protected set; }
        public object Model { get; set; }

        public abstract void Update();

        protected DataModelVisualizationViewModel CreateChild(PropertyInfo propertyInfo)
        {
            // Skip properties decorated with DataModelIgnore
            if (Attribute.IsDefined(propertyInfo, typeof(DataModelIgnoreAttribute)))
                return null;

            var dataModelPropertyAttribute = (DataModelPropertyAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute));
            // If no DataModelProperty attribute was provided, pull one out of our ass
            if (dataModelPropertyAttribute == null)
                dataModelPropertyAttribute = new DataModelPropertyAttribute {Name = propertyInfo.Name.Humanize()};

            // For primitives, create a property view model, it may be null that is fine
            if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string))
                return new DataModelPropertyViewModel(propertyInfo, dataModelPropertyAttribute, this);
            if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
                return new DataModelListViewModel(propertyInfo, dataModelPropertyAttribute, this);
            // For other value types create a child view model if the value type is not null
            if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsStruct())
            {
                var value = propertyInfo.GetValue(Model);
                if (value == null)
                    return null;

                return new DataModelViewModel(propertyInfo, value, dataModelPropertyAttribute, this);
            }

            return null;
        }

        protected DataModelVisualizationViewModel CreateChild(object value)
        {
            var dataModelPropertyAttribute = new DataModelPropertyAttribute {Name = "Unknown property"};

            // For primitives, create a property view model, it may be null that is fine
            if (value.GetType().IsPrimitive || value is string)
                return new DataModelPropertyViewModel(null, dataModelPropertyAttribute, this) {Model = value};
            // For other value types create a child view model if the value type is not null
            if (value.GetType().IsClass || value.GetType().IsStruct())
                return new DataModelViewModel(null, value, dataModelPropertyAttribute, this);

            return null;
        }
    }
}