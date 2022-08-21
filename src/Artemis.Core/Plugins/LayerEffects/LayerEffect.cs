using System;

namespace Artemis.Core.LayerEffects;

/// <summary>
///     Represents an effect that applies preprocessing and/or postprocessing to a layer
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class LayerEffect<T> : BaseLayerEffect where T : LayerEffectPropertyGroup, new()
{
    private T _properties = null!;

    /// <summary>
    ///     Gets whether all properties on this effect are initialized
    /// </summary>
    public bool PropertiesInitialized { get; internal set; }

    /// <inheritdoc />
    public override LayerEffectPropertyGroup BaseProperties => Properties;

    /// <summary>
    ///     Gets the properties of this effect.
    /// </summary>
    public T Properties
    {
        get
        {
            // I imagine a null reference here can be confusing, so lets throw an exception explaining what to do
            if (_properties == null)
                throw new InvalidOperationException("Cannot access effect properties until OnPropertiesInitialized has been called");
            return _properties;
        }
        internal set => _properties = value;
    }

    internal override void Initialize()
    {
        InitializeProperties();
    }

    private void InitializeProperties()
    {
        Properties = new T();
        Properties.Initialize(ProfileElement, null, new PropertyGroupDescriptionAttribute {Identifier = "LayerEffect"}, LayerEffectEntity.PropertyGroup);

        // Initialize will call PopulateDefaults but that is for plugin developers so can't rely on that to default IsEnabled to true
        Properties.InitializeIsEnabled();
        PropertiesInitialized = true;

        EnableLayerEffect();
    }
}