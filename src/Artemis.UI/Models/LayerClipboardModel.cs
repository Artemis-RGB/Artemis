using System;
using System.Text.Json.Serialization;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;

namespace Artemis.UI.Models;

public class LayerClipboardModel : IClipboardModel
{
    public LayerClipboardModel(Layer layer)
    {
        Layer = layer.LayerEntity;
    }

    [JsonConstructor]
    public LayerClipboardModel()
    {
    }

    public LayerEntity Layer { get; set; } = null!;

    public RenderProfileElement Paste(Folder parent)
    {
        Layer.Id = Guid.NewGuid();
        return new Layer(parent.Profile, parent, Layer, true);
    }
}