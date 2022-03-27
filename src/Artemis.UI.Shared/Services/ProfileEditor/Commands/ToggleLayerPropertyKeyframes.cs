using System;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
/// Represents a profile editor command that can be used to enable or disable keyframes on a layer property.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ToggleLayerPropertyKeyframes<T> : IProfileEditorCommand
{
    private readonly bool _enable;
    private readonly TimeSpan _time;
    private readonly LayerProperty<T> _layerProperty;
    private LayerPropertyKeyframe<T>? _keyframe;

    /// <summary>
    /// Creates a new instance of the <see cref="ToggleLayerPropertyKeyframes{T}"/> class.
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
        if (_enable)
        {
            _layerProperty.KeyframesEnabled = true;

            // If there weren't any keyframes yet, add one with the current value at the current time
            if (!_layerProperty.Keyframes.Any())
            {
                // If executed before, reuse the previous keyframe
                _keyframe ??= new LayerPropertyKeyframe<T>(_layerProperty.CurrentValue, _time, Easings.Functions.Linear, _layerProperty);
                _layerProperty.AddKeyframe(_keyframe);
            }
        }
        else
        {
            _layerProperty.KeyframesEnabled = false;
        }
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_enable)
        {
            if (_keyframe != null)
                _layerProperty.RemoveKeyframe(_keyframe);
            _layerProperty.KeyframesEnabled = false;
        }
        else
        {
            _layerProperty.KeyframesEnabled = true;
        }
    }

    #endregion
}