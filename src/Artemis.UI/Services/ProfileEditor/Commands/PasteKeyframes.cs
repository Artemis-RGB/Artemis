using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Models;
using Artemis.UI.Shared.Services.ProfileEditor;

namespace Artemis.UI.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to paste keyframes at a new position.
/// </summary>
public class PasteKeyframes : IProfileEditorCommand
{
    private readonly List<KeyframeClipboardModel> _keyframes;
    private readonly RenderProfileElement _profileElement;
    private readonly TimeSpan _startPosition;

    public PasteKeyframes(RenderProfileElement profileElement, List<KeyframeClipboardModel> keyframes, TimeSpan startPosition)
    {
        _profileElement = profileElement;
        _keyframes = keyframes;
        _startPosition = startPosition;
    }

    public List<ILayerPropertyKeyframe>? PastedKeyframes { get; set; }

    private List<ILayerPropertyKeyframe> CreateKeyframes()
    {
        List<ILayerPropertyKeyframe> result = new();

        // Delegate creating the keyframes using the model to the appropriate layer properties
        List<ILayerProperty> layerProperties = _profileElement.GetAllLayerProperties();
        foreach (KeyframeClipboardModel clipboardModel in _keyframes)
        {
            ILayerProperty? layerProperty = layerProperties.FirstOrDefault(p => p.Path == clipboardModel.Path);
            ILayerPropertyKeyframe? keyframe = layerProperty?.CreateKeyframeFromEntity(clipboardModel.Entity);
            if (keyframe != null)
                result.Add(keyframe);
        }

        // Apply the position to the keyframes
        TimeSpan positionOffset = _startPosition - result.Min(k => k.Position);
        foreach (ILayerPropertyKeyframe layerPropertyKeyframe in result)
            layerPropertyKeyframe.Position += positionOffset;

        return result;
    }

    /// <inheritdoc />
    public string DisplayName => "Paste keyframes";

    /// <inheritdoc />
    public void Execute()
    {
        PastedKeyframes ??= CreateKeyframes();
        foreach (ILayerPropertyKeyframe layerPropertyKeyframe in PastedKeyframes)
            layerPropertyKeyframe.UntypedLayerProperty.AddUntypedKeyframe(layerPropertyKeyframe);
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (PastedKeyframes == null)
            return;
        foreach (ILayerPropertyKeyframe layerPropertyKeyframe in PastedKeyframes)
            layerPropertyKeyframe.UntypedLayerProperty.RemoveUntypedKeyframe(layerPropertyKeyframe);
    }
}