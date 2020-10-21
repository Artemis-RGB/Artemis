using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelEventViewModel : DataModelVisualizationViewModel
    {
        internal DataModelEventViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath) : base(dataModel, parent, dataModelPath)
        {
        }

        public override void Update(IDataModelUIService dataModelUIService)
        {
        }

        public override object GetCurrentValue()
        {
            return null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return DisplayPath ?? Path;
        }

        internal override int GetChildDepth()
        {
            return PropertyDescription != null && !PropertyDescription.ResetsDepth ? Depth + 1 : 1;
        }
    }
}