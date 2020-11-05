using System;
using System.Linq;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    public class DataModelEventViewModel : DataModelVisualizationViewModel
    {
        private Type _displayValueType;

        internal DataModelEventViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath) : base(dataModel, parent, dataModelPath)
        {
        }

        public Type DisplayValueType
        {
            get => _displayValueType;
            set => SetAndNotify(ref _displayValueType, value);
        }

        public override void Update(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration configuration)
        {
            DisplayValueType = DataModelPath?.GetPropertyType();

            if (configuration != null)
            {
                if (configuration.CreateEventChildren)
                    PopulateProperties(dataModelUIService, configuration);
                else if (Children.Any())
                    Children.Clear();
            }
            
            // Only update children if the parent is expanded
            if (Parent != null && !Parent.IsRootViewModel && !Parent.IsVisualizationExpanded)
                return;

            foreach (DataModelVisualizationViewModel dataModelVisualizationViewModel in Children)
                dataModelVisualizationViewModel.Update(dataModelUIService, configuration);
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