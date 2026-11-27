using System;
using System.Linq;
using Artemis.Core;
using Artemis.Core.LayerEffects;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to remove a layer effect from a profile element.
/// </summary>
public class RemoveLayerEffect : IProfileEditorCommand, IDisposable
{
    private readonly BaseLayerEffect _layerEffect;
    private readonly RenderProfileElement _renderProfileElement;
    private bool _executed;
    private int _order;

    /// <summary>
    ///     Creates a new instance of the <see cref="RemoveLayerEffect" /> class.
    /// </summary>
    public RemoveLayerEffect(BaseLayerEffect layerEffect)
    {
        _renderProfileElement = layerEffect.ProfileElement;
        _layerEffect = layerEffect;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_executed)
            _layerEffect.Dispose();
    }

    /// <inheritdoc />
    public string DisplayName => "Remove layer effect";

    /// <inheritdoc />
    public void Execute()
    {
        _order = _layerEffect.Order;
        _renderProfileElement.RemoveLayerEffect(_layerEffect);
        _executed = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Removing the effect renumbered the effects below it, make room so it goes back in its old place
        foreach (BaseLayerEffect layerEffect in _renderProfileElement.LayerEffects.Where(e => e.Order >= _order))
            layerEffect.Order++;
        _layerEffect.Order = _order;

        _renderProfileElement.AddLayerEffect(_layerEffect);
        _executed = false;
    }
}