using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Artemis.Core.LayerEffects;

/// <summary>
///     Allows you to register one or more <see cref="LayerEffect{T}" />s usable by profile layers.
/// </summary>
public abstract class LayerEffectProvider : PluginFeature
{
    private readonly List<LayerEffectDescriptor> _layerEffectDescriptors;

    /// <summary>
    ///     Allows you to register one or more <see cref="LayerEffect{T}" />s usable by profile layers.
    /// </summary>
    protected LayerEffectProvider()
    {
        _layerEffectDescriptors = new List<LayerEffectDescriptor>();
        LayerEffectDescriptors = new ReadOnlyCollection<LayerEffectDescriptor>(_layerEffectDescriptors);
        Disabled += OnDisabled;
    }

    /// <summary>
    ///     A read-only collection of all layer effects added with <see cref="RegisterLayerEffectDescriptor{T}" />
    /// </summary>
    public ReadOnlyCollection<LayerEffectDescriptor> LayerEffectDescriptors { get; }

    /// <summary>
    ///     Adds a layer effect descriptor for a given layer effect, so that it appears in the UI.
    ///     <para>Note: You do not need to manually remove these on disable</para>
    /// </summary>
    /// <typeparam name="T">The type of the layer effect you wish to register</typeparam>
    /// <param name="displayName">The name to display in the UI</param>
    /// <param name="description">The description to display in the UI</param>
    /// <param name="icon">
    ///     The Material icon to display in the UI, a full reference can be found
    ///     <see href="https://materialdesignicons.com">here</see>.
    ///     <para>May also be a path to an SVG file relative to the directory of the plugin.</para>
    /// </param>
    protected void RegisterLayerEffectDescriptor<T>(string displayName, string description, string icon) where T : BaseLayerEffect
    {
        if (!IsEnabled)
            throw new ArtemisPluginFeatureException(this, "Can only add a layer effect descriptor when the plugin is enabled");

        if (icon.Contains('.'))
            icon = Plugin.ResolveRelativePath(icon);
        LayerEffectDescriptor descriptor = new(displayName, description, icon, typeof(T), this);
        _layerEffectDescriptors.Add(descriptor);
        LayerEffectStore.Add(descriptor);
    }

    private void OnDisabled(object? sender, EventArgs e)
    {
        // The store will clean up the registrations by itself, the plugin just needs to clear its own list
        _layerEffectDescriptors.Clear();
    }
}