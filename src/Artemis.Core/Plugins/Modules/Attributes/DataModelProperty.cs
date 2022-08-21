using System;

namespace Artemis.Core.Modules;

/// <summary>
///     Represents an attribute that describes a data model property
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DataModelPropertyAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the user-friendly name for this property, shown in the UI.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the user-friendly description for this property, shown in the UI.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the an optional prefix to show before displaying elements in the UI.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    ///     Gets or sets an optional affix to show behind displaying elements in the UI.
    /// </summary>
    public string? Affix { get; set; }

    /// <summary>
    ///     Gets or sets the name of list items, only applicable to enumerable data model properties
    /// </summary>
    public string? ListItemName { get; set; }

    /// <summary>
    ///     Gets or sets an optional maximum value, this value is not enforced but used for percentage calculations.
    /// </summary>
    public object? MaxValue { get; set; }

    /// <summary>
    ///     Gets or sets an optional minimum value, this value is not enforced but used for percentage calculations.
    /// </summary>
    public object? MinValue { get; set; }

    /// <summary>
    ///     Gets or sets whether this property resets the max depth of the data model, defaults to true
    /// </summary>
    public bool ResetsDepth { get; set; } = true;
}