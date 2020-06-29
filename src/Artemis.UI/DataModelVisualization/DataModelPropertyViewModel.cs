using System;
using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;

namespace Artemis.UI.DataModelVisualization
{
    public class DataModelPropertyViewModel : DataModelVisualizationViewModel
    {
        public DataModelPropertyViewModel(PropertyInfo propertyInfo, DataModelPropertyAttribute propertyDescription, DataModelVisualizationViewModel parent)
        {
            PropertyInfo = propertyInfo;
            Parent = parent;
            PropertyDescription = propertyDescription;
        }

        public bool IsListProperty { get; set; }
        public string ListDescription { get; set; }
        public Type PropertyType { get; set; }

        public override void Update()
        {
            if (PropertyInfo != null && Parent?.Model != null)
            {
                IsListProperty = false;
                Model = PropertyInfo.GetValue(Parent.Model);
                PropertyType = PropertyInfo.PropertyType;
            }
            else if (Parent is DataModelListViewModel listViewModel)
            {
                IsListProperty = true;
                ListDescription = $"List item [{listViewModel.List.IndexOf(Model)}]";
                PropertyType = Model.GetType();
            }
        }
    }
}