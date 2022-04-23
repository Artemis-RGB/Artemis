using System;
using Artemis.Core;
using Artemis.Core.LayerEffects;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
/// Represents a profile editor command that can be used to remove a layer effect from a profile element.
/// </summary>
public class RemoveLayerEffect : IProfileEditorCommand, IDisposable
{
    private readonly RenderProfileElement _renderProfileElement;
    private readonly BaseLayerEffect _layerEffect;
    private bool _executed;

    /// <summary>
    /// Creates a new instance of the <see cref="RemoveLayerEffect"/> class.
    /// </summary>
    public RemoveLayerEffect(BaseLayerEffect layerEffect)
    {
        _renderProfileElement = layerEffect.ProfileElement;
        _layerEffect = layerEffect;
    }

    /// <inheritdoc />
    public string DisplayName => "Remove layer effect";

    /// <inheritdoc />
    public void Execute()
    {
        _renderProfileElement.RemoveLayerEffect(_layerEffect);
        _executed = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _renderProfileElement.AddLayerEffect(_layerEffect);
        _executed = false;
    }
    
    /// <inheritdoc />
    public void Dispose()
    {
        if (_executed)
            _layerEffect.Dispose();
    }
}