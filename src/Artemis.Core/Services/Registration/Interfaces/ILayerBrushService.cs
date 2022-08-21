using System.Collections.Generic;
using Artemis.Core.LayerBrushes;

namespace Artemis.Core.Services;

/// <summary>
///     A service that allows you to register and retrieve layer brushes
/// </summary>
public interface ILayerBrushService : IArtemisService
{
    /// <summary>
    ///     Add a layer brush descriptor so that it is available to layers
    /// </summary>
    LayerBrushRegistration RegisterLayerBrush(LayerBrushDescriptor descriptor);

    /// <summary>
    ///     Remove a previously added layer brush descriptor so that it is no longer available
    /// </summary>
    void RemoveLayerBrush(LayerBrushRegistration registration);

    /// <summary>
    ///     Returns a list of all registered layer brush descriptors
    /// </summary>
    List<LayerBrushDescriptor> GetLayerBrushes();

    /// <summary>
    ///     Returns the descriptor of the default layer brush
    /// </summary>
    LayerBrushDescriptor? GetDefaultLayerBrush();

    /// <summary>
    ///     Applies the configured default brush to the provided <paramref name="layer" />.
    /// </summary>
    /// <param name="layer">The layer to apply the default brush to.</param>
    void ApplyDefaultBrush(Layer layer);
}