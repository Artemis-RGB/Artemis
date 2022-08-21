using System;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Shared.DataModelVisualization.Shared;

/// <summary>
///     Represents a view model that visualizes a class (POCO) data model property containing child properties
/// </summary>
public class DataModelPropertiesViewModel : DataModelVisualizationViewModel
{
    private object? _displayValue;
    private Type? _displayValueType;

    internal DataModelPropertiesViewModel(DataModel? dataModel, DataModelVisualizationViewModel? parent, DataModelPath? dataModelPath)
        : base(dataModel, parent, dataModelPath)
    {
    }

    /// <summary>
    ///     Gets the type of the property that is being visualized
    /// </summary>
    public Type? DisplayValueType
    {
        get => _displayValueType;
        private set => this.RaiseAndSetIfChanged(ref _displayValueType, value);
    }

    /// <summary>
    ///     Gets the value of the property that is being visualized
    /// </summary>
    public object? DisplayValue
    {
        get => _displayValue;
        private set => this.RaiseAndSetIfChanged(ref _displayValue, value);
    }

    /// <inheritdoc />
    public override void Update(IDataModelUIService dataModelUIService, DataModelUpdateConfiguration? configuration)
    {
        DisplayValueType = DataModelPath?.GetPropertyType();

        // Only set a display value if ToString returns useful information and not just the type name
        object? currentValue = GetCurrentValue();
        if (currentValue != null && currentValue.ToString() != currentValue.GetType().ToString())
            DisplayValue = currentValue.ToString();
        else
            DisplayValue = null;

        // Always populate properties   
        PopulateProperties(dataModelUIService, configuration);

        // Only update children if the parent is expanded
        if (Parent != null && !Parent.IsRootViewModel && !Parent.IsVisualizationExpanded)
            return;

        foreach (DataModelVisualizationViewModel dataModelVisualizationViewModel in Children)
            dataModelVisualizationViewModel.Update(dataModelUIService, configuration);
    }

    /// <inheritdoc />
    public override object? GetCurrentValue()
    {
        if (Parent == null || Parent.IsRootViewModel || IsRootViewModel)
            return DataModel;
        return base.GetCurrentValue();
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