using System;
using Artemis.Core;
using Artemis.Core.LayerEffects;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
/// Represents a profile editor command that can be used to rename a layer effect
/// </summary>
public class RenameLayerEffect : IProfileEditorCommand
{
    private readonly BaseLayerEffect _layerEffect;
    private readonly string _name;
    private readonly string _oldName;
    private readonly bool _wasRenamed;

    /// <summary>
    /// Creates a new instance of the <see cref="RenameLayerEffect"/> class.
    /// </summary>
    public RenameLayerEffect(BaseLayerEffect layerEffect, string name)
    {
        _layerEffect = layerEffect;
        _name = name;
        _oldName = layerEffect.Name;
        _wasRenamed = layerEffect.HasBeenRenamed;
    }

    /// <inheritdoc />
    public string DisplayName => "Rename layer effect";

    /// <inheritdoc />
    public void Execute()
    {
        _layerEffect.Name = _name;
        _layerEffect.HasBeenRenamed = true;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _layerEffect.Name = _oldName;
        _layerEffect.HasBeenRenamed = _wasRenamed;
    }
}