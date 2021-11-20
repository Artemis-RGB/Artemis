using System;
using System.Linq;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Shared.DataModelVisualization.Shared
{
    /// <summary>
    ///     Represents a view model that wraps a regular <see cref="DataModelPropertiesViewModel" /> but contained in
    ///     a <see cref="DataModelListViewModel" />
    /// </summary>
    public class DataModelListPropertiesViewModel : DataModelPropertiesViewModel
    {
        private object? _displayValue;
        private int _index;
        private Type? _listType;

        internal DataModelListPropertiesViewModel(Type listType, string? name) : base(null, null, null)
        {
            ListType = listType;
        }

        /// <summary>
        ///     Gets the index of the element within the list
        /// </summary>
        public int Index
        {
            get => _index;
            set => this.RaiseAndSetIfChanged(ref _index, value);
        }

        /// <summary>
        ///     Gets the type of elements contained in the list
        /// </summary>
        public Type? ListType
        {
            get => _listType;
            set => this.RaiseAndSetIfChanged(ref _listType, value);
        }

        /// <summary>
        ///     Gets the value of the property that is being visualized
        /// </summary>
        public new object? DisplayValue
        {
            get => _displayValue;
            set => this.RaiseAndSetIfChanged(ref _displayValue, value);
        }

        /// <summary>
        ///     Gets the view model that handles displaying the property
        /// </summary>
        public DataModelVisualizationViewModel? DisplayViewModel => Children.FirstOrDefault();

        /// <inheritdoc />
        public override string? DisplayPath => null;

        /// <inheritdoc />
        public override void Update(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration? configuration)
        {
            PopulateProperties(dataModelUIService, configuration);
            if (DisplayViewModel == null)
                return;

            if (IsVisualizationExpanded && !DisplayViewModel.IsVisualizationExpanded)
                DisplayViewModel.IsVisualizationExpanded = IsVisualizationExpanded;
            DisplayViewModel.Update(dataModelUIService, null);
        }

        /// <inheritdoc />
        public override object? GetCurrentValue()
        {
            return DisplayValue;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[List item {Index}] {DisplayPath ?? Path}";
        }
    }
}