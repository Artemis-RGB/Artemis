using System;
using Artemis.Core;

namespace Artemis.UI.Shared;

/// <inheritdoc />
public class PluginConfigurationDialog<T> : PluginConfigurationDialog where T : PluginConfigurationViewModel
{
    /// <inheritdoc />
    public override Type Type => typeof(T);
}

/// <summary>
///     Describes a configuration dialog for a specific plugin
/// </summary>
public abstract class PluginConfigurationDialog : IPluginConfigurationDialog
{
    /// <inheritdoc />
    public abstract Type Type { get; }
}