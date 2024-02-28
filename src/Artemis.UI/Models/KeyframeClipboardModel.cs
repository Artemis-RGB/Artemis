using System.Text.Json.Serialization;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;

namespace Artemis.UI.Models;

public class KeyframeClipboardModel: IClipboardModel
{
    public const string ClipboardDataFormat = "Artemis.Keyframes";

    [JsonConstructor]
    public KeyframeClipboardModel()
    {
    }

    public KeyframeClipboardModel(ILayerPropertyKeyframe keyframe)
    {
        KeyframeEntity entity = keyframe.GetKeyframeEntity();
        Path = keyframe.UntypedLayerProperty.Path;
        Entity = entity;
    }

    public string Path { get; set; } = null!;
    public KeyframeEntity Entity { get; set; } = null!;
}