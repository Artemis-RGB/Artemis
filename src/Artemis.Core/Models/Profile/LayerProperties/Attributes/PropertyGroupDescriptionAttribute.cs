using System;

namespace Artemis.Core;

/// <summary>
///     Represents a description attribute used to decorate layer property groups
/// </summary>
public class PropertyGroupDescriptionAttribute : Attribute
{
    /// <summary>
    ///     The identifier of this property group used for storage, if not set one will be generated based on the group name in
    ///     code
    /// </summary>
    public string? Identifier { get; set; }

    /// <summary>
    ///     The user-friendly name for this property group, shown in the UI.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The user-friendly description for this property group, shown in the UI.
    /// </summary>
    public string? Description { get; set; }
}