using System;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a view model that visualizes an event data model property
    /// </summary>
    public class DataModelEventViewModel : DataModelVisualizationViewModel
    {
        private Type? _displayValueType;

        internal DataModelEventViewModel(DataModel dataModel, DataModelVisualizationViewModel parent, DataModelPath dataModelPath) : base(dataModel, parent, dataModelPath)
        {
        }

        /// <summary>
        ///     Gets the type of event arguments this event triggers and that must be displayed as children
        /// </summary>
        public Type? DisplayValueType
        {
            get => _displayValueType;
            set => SetAndNotify(ref _displayValueType, value);
        }

        /// <inheritdoc />
        public override void Update(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration? configuration)
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

        /// <summary>
        /// Always returns <see langword="null"/> for data model events
        /// </summary>
        public override object? GetCurrentValue()
        {
            return null;
        }

        /// <inheritdoc />
        public override string? ToString()
        {
            return DisplayPath ?? Path;
        }

        internal override int GetChildDepth()
        {
            return PropertyDescription != null && !PropertyDescription.ResetsDepth ? Depth + 1 : 1;
        }
    }
}