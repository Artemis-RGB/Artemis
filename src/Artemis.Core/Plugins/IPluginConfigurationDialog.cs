using System;

namespace Artemis.Core;

/// <summary>
///     Represents a configuration dialog for a <see cref="Plugin" />
/// </summary>
public interface IPluginConfigurationDialog
{
    /// <summary>
    ///     The type of view model the tab contains
    /// </summary>
    Type Type { get; }

    /// <summary>
    ///     A value indicating whether it's mandatory to configure this plugin.
    ///     <remarks>If set to <see langword="true"/>, the dialog will open the first time the plugin is enabled.</remarks>
    /// </summary>
    bool IsMandatory { get; }
}