using System;
using Artemis.Core.LayerEffects;

namespace Artemis.UI.Shared.LayerEffects;

/// <inheritdoc />
public class LayerEffectConfigurationDialog<T> : LayerEffectConfigurationDialog where T : EffectConfigurationViewModel
{
    /// <inheritdoc />
    public LayerEffectConfigurationDialog()
    {
    }

    /// <inheritdoc />
    public LayerEffectConfigurationDialog(int dialogWidth, int dialogHeight)
    {
        DialogWidth = dialogWidth;
        DialogHeight = dialogHeight;
    }

    /// <inheritdoc />
    public override Type Type => typeof(T);
}

/// <summary>
///     Describes a UI tab for a specific layer effect
/// </summary>
public abstract class LayerEffectConfigurationDialog : ILayerEffectConfigurationDialog
{
    /// <summary>
    ///     The default width of the dialog
    /// </summary>
    public int DialogWidth { get; set; } = 800;

    /// <summary>
    ///     The default height of the dialog
    /// </summary>
    public int DialogHeight { get; set; } = 800;

    /// <summary>
    ///     The type of view model the dialog contains
    /// </summary>
    public abstract Type Type { get; }
}