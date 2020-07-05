using System.Linq;
using System.Reflection;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    public class DataModelPropertiesViewModel : DataModelVisualizationViewModel
    {
        internal DataModelPropertiesViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, PropertyInfo propertyInfo) : base(dataModel, parent, propertyInfo)
        {
        }
        
        public override void Update(IDataModelVisualizationService dataModelVisualizationService)
        {
            PopulateProperties(dataModelVisualizationService);
            foreach (var dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update(dataModelVisualizationService);
        }

        public override object GetCurrentValue()
        {
            return Parent.IsRootViewModel ? DataModel : base.GetCurrentValue();
        }

     

        private void PopulateProperties(IDataModelVisualizationService dataModelVisualizationService)
        {
            if (Children.Any())
                return;

            var modelType = Parent.IsRootViewModel ? DataModel.GetType() : PropertyInfo.PropertyType;
            foreach (var propertyInfo in modelType.GetProperties())
            {
                var child = CreateChild(dataModelVisualizationService, propertyInfo);
                if (child != null)
                    Children.Add(child);
            }
        }
    }
}