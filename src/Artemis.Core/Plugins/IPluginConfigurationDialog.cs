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
}