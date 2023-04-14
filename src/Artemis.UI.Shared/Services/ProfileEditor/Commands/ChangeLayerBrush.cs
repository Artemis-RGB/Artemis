using System;
using Artemis.Core;
using Artemis.Core.LayerBrushes;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to change the brush of a layer.
/// </summary>
public class ChangeLayerBrush : IProfileEditorCommand, IDisposable
{
    private readonly Layer _layer;
    private readonly LayerBrushDescriptor _layerBrushDescriptor;
    private readonly BaseLayerBrush? _previousBrush;
    private bool _executed;

    private BaseLayerBrush? _newBrush;

    /// <summary>
    ///     Creates a new instance of the <see cref="ChangeLayerBrush" /> class.
    /// </summary>
    public ChangeLayerBrush(Layer layer, LayerBrushDescriptor layerBrushDescriptor)
    {
        _layer = layer;
        _layerBrushDescriptor = layerBrushDescriptor;
        _previousBrush = _layer.LayerBrush;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_executed)
            _previousBrush?.Dispose();
        else
            _newBrush?.Dispose();
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Change layer brush";

    /// <inheritdoc />
    public void Execute()
    {
        // Create the new brush
        _newBrush ??= _layerBrushDescriptor.CreateInstance(_layer, null);
        // Change the brush to the new brush
        _layer.ChangeLayerBrush(_newBrush);

        _executed = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_previousBrush != null)
            _layer.ChangeLayerBrush(_previousBrush);
        _executed = false;
    }

    #endregion
}