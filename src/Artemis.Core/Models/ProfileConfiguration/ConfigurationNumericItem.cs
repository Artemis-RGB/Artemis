using Artemis.Core.Modules;

namespace Artemis.Core;

/// <summary>
///     Represents a configuration item that accepts numeric input from the user.
/// </summary>
public class ConfigurationNumericItem : ConfigurationInputItem<Numeric>
{
    /// <summary>
    ///     Gets or sets the minimum value the user can enter, or <see langword="null" /> for no minimum.
    /// </summary>
    public Numeric? Minimum
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets the maximum value the user can enter, or <see langword="null" /> for no maximum.
    /// </summary>
    public Numeric? Maximum
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the value is entered using a slider instead of a number box.
    /// </summary>
    public bool Slider
    {
        get;
        set => SetAndNotify(ref field, value);
    }

    // Numerics are exposed to the data model as floats, like most other numeric data model values
    internal override void AddDynamicChild(DataModel dataModel, string key, DataModelPropertyAttribute attribute)
    {
        dataModel.AddDynamicChild(key, (float) Value, attribute);
    }

    internal override void UpdateDynamicChild(DataModel dataModel, DynamicChild dynamicChild)
    {
        if (dynamicChild is DynamicChild<float> typedDynamicChild)
            typedDynamicChild.Value = Value;
    }
}