using System;
using Artemis.Core.LayerEffects.Placeholder;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.LayerEffects;

/// <summary>
///     A class that describes a layer effect
/// </summary>
public class LayerEffectDescriptor
{
    internal LayerEffectDescriptor(string displayName, string description, string icon, Type layerEffectType, LayerEffectProvider provider)
    {
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        LayerEffectType = layerEffectType ?? throw new ArgumentNullException(nameof(layerEffectType));
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    private LayerEffectDescriptor(LayerEffectProvider provider)
    {
        IsPlaceholder = true;
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        DisplayName = "Missing effect";
        Description = "This effect could not be loaded";
        Icon = "FileQuestion";
    }

    /// <summary>
    ///     The name that is displayed in the UI
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    ///     The description that is displayed in the UI
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     The Material icon to display in the UI, a full reference can be found
    ///     <see href="https://materialdesignicons.com">here</see>
    /// </summary>
    public string Icon { get; }

    /// <summary>
    ///     The type of the layer effect
    /// </summary>
    public Type? LayerEffectType { get; }

    /// <summary>
    ///     The plugin that provided this <see cref="LayerEffectDescriptor" />
    /// </summary>
    public LayerEffectProvider Provider { get; }

    /// <summary>
    /// Gets a boolean indicating whether this descriptor is a placeholder descriptor.
    /// </summary>
    public bool IsPlaceholder { get; }
    
    internal static LayerEffectDescriptor CreatePlaceholder(LayerEffectProvider provider)
    {
        return new LayerEffectDescriptor(provider);
    }

    /// <summary>
    ///     Creates an instance of the described effect and applies it to the render element
    /// </summary>
    public BaseLayerEffect CreateInstance(RenderProfileElement renderElement, LayerEffectEntity? entity)
    {
        if (IsPlaceholder)
        {
            if (entity == null)
                throw new ArtemisCoreException("Cannot create a placeholder for a layer effect that wasn't loaded from an entity");

            return CreatePlaceHolderInstance(renderElement, entity);
        }

        if (LayerEffectType == null)
            throw new ArtemisCoreException("Cannot create an instance of a layer effect because this descriptor is not a placeholder but is still missing its LayerEffectType");

        BaseLayerEffect effect = (BaseLayerEffect) Provider.Plugin.Resolve(LayerEffectType);
        effect.ProfileElement = renderElement;
        effect.Descriptor = this;
        if (entity != null)
        {
            effect.LayerEffectEntity = entity;
            effect.Load();
            effect.Initialize();
        }
        else
        {
            effect.LayerEffectEntity = new LayerEffectEntity();
            effect.Name = DisplayName;
            effect.Initialize();
            effect.Save();
        }

        return effect;
    }

    private BaseLayerEffect CreatePlaceHolderInstance(RenderProfileElement renderElement, LayerEffectEntity entity)
    {
        if (!IsPlaceholder)
            throw new ArtemisCoreException("Cannot create a placeholder instance using a layer effect descriptor that is not a placeholder for anything");

        PlaceholderLayerEffect effect = new(entity)
        {
            ProfileElement = renderElement,
            Descriptor = this
        };
        effect.Initialize();

        return effect;
    }
}