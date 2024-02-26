using Artemis.Core;

namespace Artemis.UI.Models;

public class LayerClipboardModel : IClipboardModel
{
    public LayerClipboardModel(Layer layer)
    {
        Layer = layer;
    }

    public Layer Layer { get; set; }
}