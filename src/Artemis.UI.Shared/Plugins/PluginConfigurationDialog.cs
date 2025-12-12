using System;
using Artemis.Core;

namespace Artemis.UI.Shared;

/// <inheritdoc />
public class PluginConfigurationDialog<T> : PluginConfigurationDialog where T : PluginConfigurationViewModel
{
    /// <summary>
    ///     Creates a new instance of the <see cref="PluginConfigurationDialog{T}"/> class.
    /// </summary>
    public PluginConfigurationDialog()
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="PluginConfigurationDialog{T}"/> class with the specified <paramref name="isMandatory"/> flag.
    /// </summary>
    /// <param name="isMandatory">A value indicating whether the configuration dialog is mandatory.</param>
    public PluginConfigurationDialog(bool isMandatory)
    {
        IsMandatory = isMandatory;
    }

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

    /// <inheritdoc />
    public bool IsMandatory { get; protected set; }
}