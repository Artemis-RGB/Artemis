﻿using System;

namespace Artemis.Core;

/// <summary>
///     Represents an attribute that describes a plugin feature
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PluginFeatureAttribute : Attribute
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
    ///     The plugins display icon that's shown in the settings see <see href="https://materialdesignicons.com" /> for
    ///     available icons
    /// </summary>
    [Obsolete("Feature icons are no longer shown in the UI.")]
    public string? Icon { get; set; }

    /// <summary>
    ///     Marks the feature to always be enabled as long as the plugin is enabled
    ///     <para>Note: always <see langword="true" /> if this is the plugin's only feature</para>
    /// </summary>
    public bool AlwaysEnabled { get; set; }
}