using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Storage.Entities.Profile;
using Humanizer;

namespace Artemis.Core;

/// <summary>
///     Represents a property group on a layer
///     <para>
///         Note: You cannot initialize property groups yourself. If properly placed and annotated, the Artemis core will
///         initialize these for you.
///     </para>
/// </summary>
public abstract class LayerPropertyGroup : IDisposable
{
    private readonly List<ILayerProperty> _layerProperties;
    private readonly List<LayerPropertyGroup> _layerPropertyGroups;
    private bool _disposed;
    private bool _isHidden;

    /// <summary>
    ///     A base constructor for a <see cref="LayerPropertyGroup" />
    /// </summary>
    protected LayerPropertyGroup()
    {
        // These are set right after construction to keep the constructor (and inherited constructs) clean
        ProfileElement = null!;
        GroupDescription = null!;
        Path = "";

        _layerProperties = new List<ILayerProperty>();
        _layerPropertyGroups = new List<LayerPropertyGroup>();

        LayerProperties = new ReadOnlyCollection<ILayerProperty>(_layerProperties);
        LayerPropertyGroups = new ReadOnlyCollection<LayerPropertyGroup>(_layerPropertyGroups);
    }

    /// <summary>
    ///     Gets the profile element (such as layer or folder) this group is associated with
    /// </summary>
    public RenderProfileElement ProfileElement { get; private set; }

    /// <summary>
    ///     Gets the description of this group
    /// </summary>
    public PropertyGroupDescriptionAttribute GroupDescription { get; private set; }

    /// <summary>
    ///     The parent group of this group
    /// </summary>
    [LayerPropertyIgnore] // Ignore the parent when selecting child groups
    public LayerPropertyGroup? Parent { get; internal set; }

    /// <summary>
    ///     Gets the unique path of the property on the render element
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    ///     Gets whether this property groups properties are all initialized
    /// </summary>
    public bool PropertiesInitialized { get; private set; }

    /// <summary>
    ///     Gets or sets whether the property is hidden in the UI
    /// </summary>
    public bool IsHidden
    {
        get => _isHidden;
        set
        {
            _isHidden = value;
            OnVisibilityChanged();
        }
    }

    /// <summary>
    ///     Gets the entity this property group uses for persistent storage
    /// </summary>
    public PropertyGroupEntity? PropertyGroupEntity { get; internal set; }

    /// <summary>
    ///     A list of all layer properties in this group
    /// </summary>
    public ReadOnlyCollection<ILayerProperty> LayerProperties { get; }

    /// <summary>
    ///     A list of al child groups in this group
    /// </summary>
    public ReadOnlyCollection<LayerPropertyGroup> LayerPropertyGroups { get; }

    /// <summary>
    ///     Recursively gets all layer properties on this group and any subgroups
    /// </summary>
    public IReadOnlyCollection<ILayerProperty> GetAllLayerProperties()
    {
        if (_disposed)
            throw new ObjectDisposedException("LayerPropertyGroup");

        if (!PropertiesInitialized)
            return new List<ILayerProperty>();

        List<ILayerProperty> result = new(LayerProperties);
        foreach (LayerPropertyGroup layerPropertyGroup in LayerPropertyGroups)
            result.AddRange(layerPropertyGroup.GetAllLayerProperties());

        return result.AsReadOnly();
    }

    /// <summary>
    ///     Applies the default value to all layer properties
    /// </summary>
    public void ResetAllLayerProperties()
    {
        foreach (ILayerProperty layerProperty in GetAllLayerProperties())
            layerProperty.ApplyDefaultValue();
    }

    /// <summary>
    ///     Occurs when the property group has initialized all its children
    /// </summary>
    public event EventHandler? PropertyGroupInitialized;

    /// <summary>
    ///     Occurs when one of the current value of one of the layer properties in this group changes by some form of input
    ///     <para>Note: Will not trigger on properties in child groups</para>
    /// </summary>
    public event EventHandler<LayerPropertyEventArgs>? LayerPropertyOnCurrentValueSet;

    /// <summary>
    ///     Occurs when the <see cref="IsHidden" /> value of the layer property was updated
    /// </summary>
    public event EventHandler? VisibilityChanged;

    /// <summary>
    ///     Called before property group is activated to allow you to populate <see cref="LayerProperty{T}.DefaultValue" /> on
    ///     the properties you want
    /// </summary>
    protected abstract void PopulateDefaults();

    /// <summary>
    ///     Called when the property group is activated
    /// </summary>
    protected abstract void EnableProperties();

    /// <summary>
    ///     Called when the property group is deactivated (either the profile unloaded or the related brush/effect was removed)
    /// </summary>
    protected abstract void DisableProperties();

    /// <summary>
    ///     Called when the property group and all its layer properties have been initialized
    /// </summary>
    protected virtual void OnPropertyGroupInitialized()
    {
        PropertyGroupInitialized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <see langword="true" /> to release both managed and unmanaged resources;
    ///     <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposed = true;
            DisableProperties();

            foreach (ILayerProperty layerProperty in _layerProperties)
                layerProperty.Dispose();
            foreach (LayerPropertyGroup layerPropertyGroup in _layerPropertyGroups)
                layerPropertyGroup.Dispose();
        }
    }

    internal void Initialize(RenderProfileElement profileElement, LayerPropertyGroup? parent, PropertyGroupDescriptionAttribute groupDescription, PropertyGroupEntity? propertyGroupEntity)
    {
        if (groupDescription.Identifier == null)
            throw new ArtemisCoreException("Can't initialize a property group without an identifier");

        // Doubt this will happen but let's make sure
        if (PropertiesInitialized)
            throw new ArtemisCoreException("Layer property group already initialized, wut");

        ProfileElement = profileElement;
        Parent = parent;
        GroupDescription = groupDescription;
        PropertyGroupEntity = propertyGroupEntity ?? new PropertyGroupEntity {Identifier = groupDescription.Identifier};
        Path = parent != null ? parent.Path + "." + groupDescription.Identifier : groupDescription.Identifier;

        // Get all properties implementing ILayerProperty or LayerPropertyGroup
        foreach (PropertyInfo propertyInfo in GetType().GetProperties())
        {
            if (Attribute.IsDefined(propertyInfo, typeof(LayerPropertyIgnoreAttribute)))
                continue;

            if (typeof(ILayerProperty).IsAssignableFrom(propertyInfo.PropertyType))
            {
                PropertyDescriptionAttribute? propertyDescription =
                    (PropertyDescriptionAttribute?) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyDescriptionAttribute));
                InitializeProperty(propertyInfo, propertyDescription ?? new PropertyDescriptionAttribute());
            }
            else if (typeof(LayerPropertyGroup).IsAssignableFrom(propertyInfo.PropertyType))
            {
                PropertyGroupDescriptionAttribute? propertyGroupDescription =
                    (PropertyGroupDescriptionAttribute?) Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyGroupDescriptionAttribute));
                InitializeChildGroup(propertyInfo, propertyGroupDescription ?? new PropertyGroupDescriptionAttribute());
            }
        }

        // Request the property group to populate defaults
        PopulateDefaults();

        // Load the layer properties after defaults have been applied
        foreach (ILayerProperty layerProperty in _layerProperties)
            layerProperty.Load();

        EnableProperties();
        PropertiesInitialized = true;
        OnPropertyGroupInitialized();
    }

    internal void ApplyToEntity()
    {
        if (!PropertiesInitialized || PropertyGroupEntity == null)
            return;

        foreach (ILayerProperty layerProperty in LayerProperties)
            layerProperty.Save();

        PropertyGroupEntity.PropertyGroups.Clear();
        foreach (LayerPropertyGroup layerPropertyGroup in LayerPropertyGroups)
        {
            layerPropertyGroup.ApplyToEntity();
            if (layerPropertyGroup.PropertyGroupEntity != null)
                PropertyGroupEntity.PropertyGroups.Add(layerPropertyGroup.PropertyGroupEntity);
        }
    }

    internal void Update(Timeline timeline)
    {
        foreach (ILayerProperty layerProperty in LayerProperties)
            layerProperty.Update(timeline);
        foreach (LayerPropertyGroup layerPropertyGroup in LayerPropertyGroups)
            layerPropertyGroup.Update(timeline);
    }

    internal void MoveLayerProperty(ILayerProperty layerProperty, int index)
    {
        if (!_layerProperties.Contains(layerProperty))
            return;

        _layerProperties.Remove(layerProperty);
        _layerProperties.Insert(index, layerProperty);
    }

    internal virtual void OnVisibilityChanged()
    {
        VisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    internal virtual void OnLayerPropertyOnCurrentValueSet(LayerPropertyEventArgs e)
    {
        Parent?.OnLayerPropertyOnCurrentValueSet(e);
        LayerPropertyOnCurrentValueSet?.Invoke(this, e);
    }

    private void InitializeProperty(PropertyInfo propertyInfo, PropertyDescriptionAttribute propertyDescription)
    {
        // Ensure the description has an identifier and name, if not this is a good point to set it based on the property info
        if (string.IsNullOrWhiteSpace(propertyDescription.Identifier))
            propertyDescription.Identifier = propertyInfo.Name;
        if (string.IsNullOrWhiteSpace(propertyDescription.Name))
            propertyDescription.Name = propertyInfo.Name.Humanize();

        if (!typeof(ILayerProperty).IsAssignableFrom(propertyInfo.PropertyType))
            throw new ArtemisPluginException($"Property with PropertyDescription attribute must be of type ILayerProperty: {propertyDescription.Identifier}");
        if (Activator.CreateInstance(propertyInfo.PropertyType, true) is not ILayerProperty instance)
            throw new ArtemisPluginException($"Failed to create instance of layer property: {propertyDescription.Identifier}");

        PropertyEntity entity = GetPropertyEntity(propertyDescription.Identifier, out bool fromStorage);
        instance.Initialize(ProfileElement, this, entity, fromStorage, propertyDescription);
        propertyInfo.SetValue(this, instance);

        _layerProperties.Add(instance);
    }

    private void InitializeChildGroup(PropertyInfo propertyInfo, PropertyGroupDescriptionAttribute propertyGroupDescription)
    {
        // Ensure the description has an identifier and name name, if not this is a good point to set it based on the property info
        if (string.IsNullOrWhiteSpace(propertyGroupDescription.Identifier))
            propertyGroupDescription.Identifier = propertyInfo.Name;
        if (string.IsNullOrWhiteSpace(propertyGroupDescription.Name))
            propertyGroupDescription.Name = propertyInfo.Name.Humanize();

        if (!typeof(LayerPropertyGroup).IsAssignableFrom(propertyInfo.PropertyType))
            throw new ArtemisPluginException($"Property with PropertyGroupDescription attribute must be of type LayerPropertyGroup: {propertyGroupDescription.Identifier}");
        if (!(Activator.CreateInstance(propertyInfo.PropertyType) is LayerPropertyGroup instance))
            throw new ArtemisPluginException($"Failed to create instance of layer property group: {propertyGroupDescription.Identifier}");

        PropertyGroupEntity entity = GetPropertyGroupEntity(propertyGroupDescription.Identifier);
        instance.Initialize(ProfileElement, this, propertyGroupDescription, entity);

        propertyInfo.SetValue(this, instance);
        _layerPropertyGroups.Add(instance);
    }

    private PropertyEntity GetPropertyEntity(string identifier, out bool fromStorage)
    {
        if (PropertyGroupEntity == null)
            throw new ArtemisCoreException($"Can't execute {nameof(GetPropertyEntity)} without {nameof(PropertyGroupEntity)} being setup");

        PropertyEntity? entity = PropertyGroupEntity.Properties.FirstOrDefault(p => p.Identifier == identifier);
        fromStorage = entity != null;
        if (entity == null)
        {
            entity = new PropertyEntity {Identifier = identifier};
            PropertyGroupEntity.Properties.Add(entity);
        }

        return entity;
    }

    private PropertyGroupEntity GetPropertyGroupEntity(string identifier)
    {
        if (PropertyGroupEntity == null)
            throw new ArtemisCoreException($"Can't execute {nameof(GetPropertyGroupEntity)} without {nameof(PropertyGroupEntity)} being setup");

        return PropertyGroupEntity.PropertyGroups.FirstOrDefault(g => g.Identifier == identifier) ?? new PropertyGroupEntity {Identifier = identifier};
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}