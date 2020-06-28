using System;
using System.Reflection;
using Artemis.Core.Extensions;
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

        public DataModelViewModel(PropertyInfo propertyInfo, object model, DataModelPropertyAttribute propertyDescription, DataModelViewModel parent)
        {
            PropertyInfo = propertyInfo;
            Model = model;
            PropertyDescription = propertyDescription;
            Parent = parent;
            Children = new BindableCollection<DataModelVisualizationViewModel>();

            PopulateProperties();
        }

        public object Model { get; private set; }
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

                // For primitives, create a property view model, it may be null that is fine
                if (propertyInfo.PropertyType.IsPrimitive || propertyInfo.PropertyType == typeof(string))
                {
                    // This may be slower than avoiding generics and Activator.CreateInstance but it allows for expression trees inside the VM we're creating
                    // here, this means slow creation but fast updates after that
                    var viewModelType = typeof(DataModelPropertyViewModel<,>).MakeGenericType(Model.GetType(), propertyInfo.PropertyType);
                    var viewModel = (DataModelVisualizationViewModel) Activator.CreateInstance(viewModelType, propertyInfo, dataModelPropertyAttribute, this);
                    Children.Add(viewModel);
                }
                // For other value types create a child view model if the value type is not null
                else if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsStruct())
                {
                    var value = propertyInfo.GetValue(Model);
                    if (value == null)
                        continue;

                    Children.Add(new DataModelViewModel(propertyInfo, value, dataModelPropertyAttribute, this));
                }
            }
        }

        public override void Update()
        {
            if (PropertyInfo != null && PropertyInfo.PropertyType.IsStruct())
                Model = PropertyInfo.GetValue(Parent.Model);

            foreach (var dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update();
        }
    }
}