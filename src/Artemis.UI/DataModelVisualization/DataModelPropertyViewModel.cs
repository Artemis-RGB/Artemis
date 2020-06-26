using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using FastMember;

namespace Artemis.UI.DataModelVisualization
{
    public class DataModelPropertyViewModel : DataModelVisualizationViewModel
    {
        private readonly ObjectAccessor _accessor;

        public DataModelPropertyViewModel(PropertyInfo propertyInfo, DataModelPropertyAttribute propertyDescription, DataModelViewModel parent)
        {
            _accessor = ObjectAccessor.Create(parent.Model);

            PropertyInfo = propertyInfo;
            Parent = parent;
            PropertyDescription = propertyDescription;
        }

        public PropertyInfo PropertyInfo { get; }
        public object Value => _accessor[PropertyInfo.Name];
    }
}