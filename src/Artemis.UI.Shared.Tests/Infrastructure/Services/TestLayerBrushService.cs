using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;

namespace Artemis.UI.Shared.Tests.Infrastructure.Services;

/// <summary>
///     Applies a brush of the test's choosing as the default brush.
/// </summary>
public class TestLayerBrushService : ILayerBrushService
{
    /// <summary>
    ///     Gets or sets the brush <see cref="ApplyDefaultBrush" /> gives layers, none if <see langword="null" />
    /// </summary>
    public LayerBrushDescriptor? DefaultBrush { get; set; }

    public LayerBrushDescriptor? GetDefaultLayerBrush()
    {
        return DefaultBrush;
    }

    public void ApplyDefaultBrush(Layer layer)
    {
        // Same as LayerBrushService
        if (DefaultBrush != null)
            layer.ChangeLayerBrush(DefaultBrush.CreateInstance(layer, null));
    }

    public LayerBrushRegistration RegisterLayerBrush(LayerBrushDescriptor descriptor) => throw new NotSupportedException();
    public void RemoveLayerBrush(LayerBrushRegistration registration) => throw new NotSupportedException();
    public List<LayerBrushDescriptor> GetLayerBrushes() => throw new NotSupportedException();
}
