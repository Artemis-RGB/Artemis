using System.Text.Json.Serialization;

namespace Artemis.UI.Models;

[JsonDerivedType(typeof(LayerClipboardModel), "ClipboardLayer")]
[JsonDerivedType(typeof(FolderClipboardModel), "ClipboardFolder")]
[JsonDerivedType(typeof(KeyframeClipboardModel), "ClipboardKeyframe")]
[JsonDerivedType(typeof(NodesClipboardModel), "ClipboardNodes")]
public interface IClipboardModel
{
}