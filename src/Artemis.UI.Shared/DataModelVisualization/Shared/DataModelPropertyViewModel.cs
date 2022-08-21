using System;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Shared.DataModelVisualization.Shared;

/// <summary>
///     Represents a view model that visualizes a single data model property contained in a
///     <see cref="DataModelPropertiesViewModel" />
/// </summary>
public class DataModelPropertyViewModel : DataModelVisualizationViewModel
{
    private object? _displayValue;
    private Type? _displayValueType;
    private DataModelDisplayViewModel? _displayViewModel;

    internal DataModelPropertyViewModel(DataModel? dataModel, DataModelVisualizationViewModel? parent, DataModelPath? dataModelPath)
        : base(dataModel, parent, dataModelPath)
    {
    }

    /// <summary>
    ///     Gets the value of the property that is being visualized
    /// </summary>
    public object? DisplayValue
    {
        get => _displayValue;
        internal set => this.RaiseAndSetIfChanged(ref _displayValue, value);
    }

    /// <summary>
    ///     Gets the type of the property that is being visualized
    /// </summary>
    public Type? DisplayValueType
    {
        get => _displayValueType;
        protected set => this.RaiseAndSetIfChanged(ref _displayValueType, value);
    }

    /// <summary>
    ///     Gets the view model used to display the display value
    /// </summary>
    public DataModelDisplayViewModel? DisplayViewModel
    {
        get => _displayViewModel;
        internal set => this.RaiseAndSetIfChanged(ref _displayViewModel, value);
    }

    /// <inheritdoc />
    public override void Update(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration? configuration)
    {
        if (Parent != null && !Parent.IsVisualizationExpanded && !Parent.IsRootViewModel)
            return;

        if (DisplayViewModel == null)
        {
            Type? propertyType = DataModelPath?.GetPropertyType();
            if (propertyType != null)
            {
                DisplayViewModel = dataModelUIService.GetDataModelDisplayViewModel(propertyType, PropertyDescription, true);
                if (DisplayViewModel != null)
                    DisplayViewModel.PropertyDescription = DataModelPath?.GetPropertyDescription();
            }
        }

        DisplayValue = GetCurrentValue();
        DisplayValueType = DisplayValue != null ? DisplayValue.GetType() : DataModelPath?.GetPropertyType();

        DisplayViewModel?.UpdateValue(DisplayValue);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (DisplayValueType != null)
            return $"[{DisplayValueType.Name}] {DisplayPath ?? Path} - {DisplayValue}";
        return $"{DisplayPath ?? Path} - {DisplayValue}";
    }
}