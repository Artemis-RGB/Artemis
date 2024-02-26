using System.Text.Json.Serialization;

namespace Artemis.UI.Models;

[JsonDerivedType(typeof(LayerClipboardModel))]
[JsonDerivedType(typeof(FolderClipboardModel))]
[JsonDerivedType(typeof(KeyframeClipboardModel))]
[JsonDerivedType(typeof(NodesClipboardModel))]
public interface IClipboardModel
{
}