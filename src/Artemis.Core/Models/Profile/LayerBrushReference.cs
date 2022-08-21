using Artemis.Core.LayerBrushes;

namespace Artemis.Core;

/// <summary>
///     A reference to a <see cref="LayerBrushDescriptor" />
/// </summary>
public class LayerBrushReference
{
    /// <summary>
    ///     Creates a new instance of the <see cref="LayerBrushReference" /> class
    /// </summary>
    public LayerBrushReference()
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="LayerBrushReference" /> class
    /// </summary>
    /// <param name="descriptor">The descriptor to point the new reference at</param>
    public LayerBrushReference(LayerBrushDescriptor descriptor)
    {
        LayerBrushProviderId = descriptor.Provider.Id;
        BrushType = descriptor.LayerBrushType.Name;
    }

    /// <summary>
    ///     The ID of the layer brush provided the brush was provided by
    /// </summary>
    public string? LayerBrushProviderId { get; set; }

    /// <summary>
    ///     The full type name of the brush descriptor
    /// </summary>
    public string? BrushType { get; set; }
}