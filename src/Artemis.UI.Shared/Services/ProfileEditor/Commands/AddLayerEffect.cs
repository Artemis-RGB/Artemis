using System;
using Artemis.Core;
using Artemis.Core.LayerEffects;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to add a layer effect to a profile element.
/// </summary>
public class AddLayerEffect : IProfileEditorCommand, IDisposable
{
    private readonly BaseLayerEffect _layerEffect;
    private readonly RenderProfileElement _renderProfileElement;
    private bool _executed;

    /// <summary>
    ///     Creates a new instance of the <see cref="AddLayerEffect" /> class.
    /// </summary>
    public AddLayerEffect(RenderProfileElement renderProfileElement, BaseLayerEffect layerEffect)
    {
        _renderProfileElement = renderProfileElement;
        _layerEffect = layerEffect;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_executed)
            _layerEffect.Dispose();
    }

    /// <inheritdoc />
    public string DisplayName => "Add layer effect";

    /// <inheritdoc />
    public void Execute()
    {
        _renderProfileElement.AddLayerEffect(_layerEffect);
        _executed = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _renderProfileElement.RemoveLayerEffect(_layerEffect);
        _executed = false;
    }
}