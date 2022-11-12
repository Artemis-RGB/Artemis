using System;
using Artemis.Core.LayerEffects;
using Artemis.Core.LayerEffects.Placeholder;

namespace Artemis.Core;

/// <summary>
///     Represents a layer effect registration
/// </summary>
public class LayerEffectRegistration
{
    internal LayerEffectRegistration(LayerEffectDescriptor descriptor, PluginFeature pluginFeature)
    {
        LayerEffectDescriptor = descriptor;
        PluginFeature = pluginFeature;

        PluginFeature.Disabled += OnDisabled;
    }

    /// <summary>
    ///     Gets the layer effect descriptor that has been registered
    /// </summary>
    public LayerEffectDescriptor LayerEffectDescriptor { get; }

    /// <summary>
    ///     Gets the plugin the layer effect is associated with
    /// </summary>
    public PluginFeature PluginFeature { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the registration is in the internal Core store
    /// </summary>
    public bool IsInStore { get; internal set; }

    private void OnDisabled(object? sender, EventArgs e)
    {
        PluginFeature.Disabled -= OnDisabled;
        if (IsInStore)
            LayerEffectStore.Remove(this);
    }

    /// <summary>
    ///     Determines whether the provided placeholder matches this event.
    /// </summary>
    /// <param name="placeholder">The placeholder to check</param>
    /// <returns><see langword="true" /> if the placeholder is for the provided layer effect registration, otherwise <see langword="false" />.</returns>
    internal bool Matches(PlaceholderLayerEffect placeholder)
    {
        return placeholder.OriginalEntity.ProviderId == PluginFeature.Id &&
               placeholder.OriginalEntity.EffectType == LayerEffectDescriptor.LayerEffectType?.FullName;
    }

    /// <summary>
    ///     Determines whether the provided layer effect matches this event.
    /// </summary>
    /// <param name="layerEffect">The layer effect to check</param>
    /// <returns><see langword="true" /> if the placeholder is for the provided layer effect registration, otherwise <see langword="false" />.</returns>
    internal bool Matches(BaseLayerEffect layerEffect)
    {
        return layerEffect.Descriptor == LayerEffectDescriptor;
    }
}