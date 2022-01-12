using Artemis.Core;
using Artemis.Core.LayerBrushes;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to change the brush of a layer.
/// </summary>
public class ChangeLayerBrush : IProfileEditorCommand
{
    private readonly Layer _layer;
    private readonly LayerBrushDescriptor _layerBrushDescriptor;
    private readonly LayerBrushDescriptor? _previousDescriptor;

    /// <summary>
    ///     Creates a new instance of the <see cref="ChangeLayerBrush" /> class.
    /// </summary>
    public ChangeLayerBrush(Layer layer, LayerBrushDescriptor layerBrushDescriptor)
    {
        _layer = layer;
        _layerBrushDescriptor = layerBrushDescriptor;
        _previousDescriptor = layer.LayerBrush?.Descriptor;
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Change layer brush";

    /// <inheritdoc />
    public void Execute()
    {
        _layer.ChangeLayerBrush(_layerBrushDescriptor);
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_previousDescriptor != null)
            _layer.ChangeLayerBrush(_previousDescriptor);
    }

    #endregion
}