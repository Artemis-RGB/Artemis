using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelPropertiesViewModel : DataModelVisualizationViewModel
    {
        internal DataModelPropertiesViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath) : base(dataModel, parent, dataModelPath)
        {
        }

        public override void Update(IDataModelUIService dataModelUIService)
        {
            // Always populate properties
            PopulateProperties(dataModelUIService, null);

            // Only update children if the parent is expanded
            if (Parent != null && !Parent.IsVisualizationExpanded && !Parent.IsRootViewModel)
                return;

            foreach (var dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update(dataModelUIService);
        }

        public override object GetCurrentValue()
        {
            return Parent.IsRootViewModel ? DataModel : base.GetCurrentValue();
        }

        internal override int GetChildDepth()
        {
            return PropertyDescription != null && !PropertyDescription.ResetsDepth ? Depth + 1 : 1;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayPath ?? Path;
        }
    }
}