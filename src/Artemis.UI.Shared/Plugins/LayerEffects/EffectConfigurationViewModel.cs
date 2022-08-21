using System;
using System.Threading.Tasks;
using Artemis.Core.LayerEffects;

namespace Artemis.UI.Shared.LayerEffects;

/// <summary>
///     Represents a view model for an effect configuration window
/// </summary>
public abstract class EffectConfigurationViewModel : ValidatableViewModelBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="EffectConfigurationViewModel" /> class
    /// </summary>
    /// <param name="layerEffect"></param>
    protected EffectConfigurationViewModel(BaseLayerEffect layerEffect)
    {
        LayerEffect = layerEffect;
    }

    /// <summary>
    ///     Gets the layer effect this view model is associated with
    /// </summary>
    public BaseLayerEffect LayerEffect { get; }

    /// <summary>
    ///     Closes the dialog
    /// </summary>
    public void RequestClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Called when the window wants to close, returning <see langword="false" /> will cause the window to stay open.
    /// </summary>
    /// <returns><see langword="true" /> if the window may close; otherwise <see langword="false" />.</returns>
    public virtual bool CanClose()
    {
        return true;
    }

    /// <summary>
    ///     Called when the window wants to close, returning <see langword="false" /> will cause the window to stay open.
    /// </summary>
    /// <returns>A task <see langword="true" /> if the window may close; otherwise <see langword="false" />.</returns>
    public virtual Task<bool> CanCloseAsync()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    ///     Occurs when a close was requested
    /// </summary>
    public event EventHandler? CloseRequested;
}