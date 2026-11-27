using System;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to enable or disable keyframes on a layer property.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ToggleLayerPropertyKeyframes<T> : IProfileEditorCommand
{
    private readonly bool _enable;
    private readonly LayerProperty<T> _layerProperty;
    private readonly TimeSpan _time;
    private LayerPropertyKeyframe<T>? _keyframe;
    private bool _addedKeyframe;
    private bool _wasEnabled;

    /// <summary>
    ///     Creates a new instance of the <see cref="ToggleLayerPropertyKeyframes{T}" /> class.
    /// </summary>
    public ToggleLayerPropertyKeyframes(LayerProperty<T> layerProperty, bool enable, TimeSpan time)
    {
        _layerProperty = layerProperty;
        _enable = enable;
        _time = time;
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => _enable ? "Enable keyframes" : "Disable keyframes";

    /// <inheritdoc />
    public void Execute()
    {
        _wasEnabled = _layerProperty.KeyframesEnabled;
        _addedKeyframe = false;
        _layerProperty.KeyframesEnabled = _enable;

        // If there weren't any keyframes yet, add one with the current value at the current time
        if (_enable && !_layerProperty.Keyframes.Any())
        {
            // If executed before, reuse the previous keyframe
            _keyframe ??= new LayerPropertyKeyframe<T>(_layerProperty.CurrentValue, _time, Easings.Functions.Linear, _layerProperty);
            _layerProperty.AddKeyframe(_keyframe);
            _addedKeyframe = true;
        }
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_addedKeyframe && _keyframe != null)
            _layerProperty.RemoveKeyframe(_keyframe);
        _layerProperty.KeyframesEnabled = _wasEnabled;
    }

    #endregion
}