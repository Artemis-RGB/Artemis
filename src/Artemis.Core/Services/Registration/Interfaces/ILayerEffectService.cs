using System.Collections.Generic;
using Artemis.Core.LayerEffects;

namespace Artemis.Core.Services;

/// <summary>
///     A service that allows you to register and retrieve layer brushes
/// </summary>
public interface ILayerEffectService : IArtemisService
{
    /// <summary>
    ///     Add an effect descriptor so that it is available to profile elements
    /// </summary>
    LayerEffectRegistration RegisterLayerEffect(LayerEffectDescriptor descriptor);

    /// <summary>
    ///     Remove a previously added layer effect descriptor so that it is no longer available
    /// </summary>
    void RemoveLayerEffect(LayerEffectRegistration registration);

    /// <summary>
    ///     Returns a list of all registered layer effect descriptors
    /// </summary>
    List<LayerEffectDescriptor> GetLayerEffects();
}