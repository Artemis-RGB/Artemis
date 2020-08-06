using System.Collections.Generic;
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
            // Always populate properties
            PopulateProperties(dataModelVisualizationService);

            // Only update children if the parent is expanded
            if (Parent != null && !Parent.IsVisualizationExpanded && !Parent.IsRootViewModel)
                return;

            foreach (var dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update(dataModelVisualizationService);
        }

        public override object GetCurrentValue()
        {
            return Parent.IsRootViewModel ? DataModel : base.GetCurrentValue();
        }

        private void PopulateProperties(IDataModelVisualizationService dataModelVisualizationService)
        {
            if (IsRootViewModel)
                return;

            // Add missing children
            var modelType = Parent.IsRootViewModel ? DataModel.GetType() : PropertyInfo.PropertyType;
            foreach (var propertyInfo in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Children.Any(c => c.PropertyInfo.Equals(propertyInfo)))
                    continue;

                var child = CreateChild(dataModelVisualizationService, propertyInfo, GetChildDepth());
                if (child != null)
                    Children.Add(child);
            }

            // Remove children that should be hidden
            var childList = new List<DataModelVisualizationViewModel>(Children);
            var hiddenProperties = DataModel.GetHiddenProperties();
            foreach (var dataModelVisualizationViewModel in childList)
            {
                if (hiddenProperties.Contains(dataModelVisualizationViewModel.PropertyInfo))
                    Children.Remove(dataModelVisualizationViewModel);
            }
        }

        protected int GetChildDepth()
        {
            return PropertyDescription != null && !PropertyDescription.ResetsDepth ? Depth + 1 : 1;
        }
    }
}