using System;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Humanizer;
using Stylet;

namespace Artemis.UI.DataModelVisualization
{
    public class DataModelViewModel : DataModelVisualizationViewModel
    {
        public DataModelViewModel()
        {
            Children = new BindableCollection<DataModelVisualizationViewModel>();
        }

        public DataModelViewModel(object model, DataModelPropertyAttribute propertyDescription, DataModelViewModel parent)
        {
            Model = model;
            PropertyDescription = propertyDescription;
            Parent = parent;
            Children = new BindableCollection<DataModelVisualizationViewModel>();

            PopulateProperties();
        }

        public object Model { get; }
        public BindableCollection<DataModelVisualizationViewModel> Children { get; set; }

        public void PopulateProperties()
        {
            Children.Clear();
            foreach (var propertyInfo in Model.GetType().GetProperties())
            {
                // Skip properties decorated with DataModelIgnore
                if (Attribute.IsDefined(propertyInfo, typeof(DataModelIgnoreAttribute)))
                    continue;

                var dataModelPropertyAttribute = (DataModelPropertyAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute));
                // If no DataModelProperty attribute was provided, pull one out of our ass
                if (dataModelPropertyAttribute == null)
                    dataModelPropertyAttribute = new DataModelPropertyAttribute {Name = propertyInfo.Name.Humanize()};

                // For value types create a child view model if the value type is not null
                if (propertyInfo.PropertyType.IsValueType)
                {
                    var value = propertyInfo.GetValue(Model);
                    if (value == null)
                        continue;

                    Children.Add(new DataModelViewModel(value, dataModelPropertyAttribute, this));
                }
                // For primitives, create a property view model, it may be null that is fine
                else if (propertyInfo.PropertyType.IsPrimitive)
                {
                    Children.Add(new DataModelPropertyViewModel(propertyInfo, dataModelPropertyAttribute, this));
                }
            }
        }
    }
}