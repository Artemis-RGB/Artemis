using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels.Attributes;
using Stylet;

namespace Artemis.UI.DataModelVisualization
{
    public abstract class DataModelVisualizationViewModel : PropertyChangedBase
    {
        public PropertyInfo PropertyInfo { get; protected set; }
        public DataModelPropertyAttribute PropertyDescription { get; protected set; }
        public DataModelViewModel Parent { get; protected set; }

        public abstract void Update();
    }
}