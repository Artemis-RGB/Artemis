using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;

namespace Artemis.UI.Models;

public class KeyframeClipboardModel
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