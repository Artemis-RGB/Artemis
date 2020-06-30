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
        
        public override void Update()
        {
            if (PropertyInfo != null && Parent?.Model != null)
                Model = PropertyInfo.GetValue(Parent.Model);

            UpdateListStatus();
        }
    }
}