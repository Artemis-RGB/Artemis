﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Storage.Entities.Profile;
using Humanizer;

namespace Artemis.Core
{
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
            GroupDescription = null!;
            Feature = null!;
            ProfileElement = null!;
            Path = null!;

            _layerProperties = new List<ILayerProperty>();
            _layerPropertyGroups = new List<LayerPropertyGroup>();
        }

        /// <summary>
        ///     Gets the description of this group
        /// </summary>
        public PropertyGroupDescriptionAttribute GroupDescription { get; internal set; }

        /// <summary>
        ///     Gets the plugin feature this group is associated with
        /// </summary>
        public PluginFeature Feature { get; set; }

        /// <summary>
        ///     Gets the profile element (such as layer or folder) this group is associated with
        /// </summary>
        public RenderProfileElement ProfileElement { get; internal set; }

        /// <summary>
        ///     The parent group of this group
        /// </summary>
        [LayerPropertyIgnore]
        public LayerPropertyGroup? Parent { get; internal set; }

        /// <summary>
        ///     The path of this property group
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        ///     Gets whether this property groups properties are all initialized
        /// </summary>
        public bool PropertiesInitialized { get; private set; }

        /// <summary>
        ///     The layer brush this property group belongs to
        /// </summary>
        public BaseLayerBrush? LayerBrush { get; internal set; }

        /// <summary>
        ///     The layer effect this property group belongs to
        /// </summary>
        public BaseLayerEffect? LayerEffect { get; internal set; }

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
        ///     A list of all layer properties in this group
        /// </summary>
        public ReadOnlyCollection<ILayerProperty> LayerProperties => _layerProperties.AsReadOnly();

        /// <summary>
        ///     A list of al child groups in this group
        /// </summary>
        public ReadOnlyCollection<LayerPropertyGroup> LayerPropertyGroups => _layerPropertyGroups.AsReadOnly();

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

        internal void Initialize(RenderProfileElement profileElement, string path, PluginFeature feature)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Doubt this will happen but let's make sure
            if (PropertiesInitialized)
                throw new ArtemisCoreException("Layer property group already initialized, wut");

            Feature = feature ?? throw new ArgumentNullException(nameof(feature));
            ProfileElement = profileElement ?? throw new ArgumentNullException(nameof(profileElement));
            Path = path.TrimEnd('.');

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
            if (!PropertiesInitialized)
                return;

            foreach (ILayerProperty layerProperty in LayerProperties)
                layerProperty.Save();

            foreach (LayerPropertyGroup layerPropertyGroup in LayerPropertyGroups)
                layerPropertyGroup.ApplyToEntity();
        }

        internal void Update(Timeline timeline)
        {
            foreach (ILayerProperty layerProperty in LayerProperties)
                layerProperty.Update(timeline);
            foreach (LayerPropertyGroup layerPropertyGroup in LayerPropertyGroups)
                layerPropertyGroup.Update(timeline);
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
            string path = $"{Path}.{propertyInfo.Name}";

            if (!typeof(ILayerProperty).IsAssignableFrom(propertyInfo.PropertyType))
                throw new ArtemisPluginException($"Layer property with PropertyDescription attribute must be of type LayerProperty at {path}");

            if (!(Activator.CreateInstance(propertyInfo.PropertyType, true) is ILayerProperty instance))
                throw new ArtemisPluginException($"Failed to create instance of layer property at {path}");

            // Ensure the description has a name, if not this is a good point to set it based on the property info
            if (string.IsNullOrWhiteSpace(propertyDescription.Name))
                propertyDescription.Name = propertyInfo.Name.Humanize();

            PropertyEntity entity = GetPropertyEntity(ProfileElement, path, out bool fromStorage);
            instance.Initialize(ProfileElement, this, entity, fromStorage, propertyDescription, path);
            propertyInfo.SetValue(this, instance);
            _layerProperties.Add(instance);
        }

        private void InitializeChildGroup(PropertyInfo propertyInfo, PropertyGroupDescriptionAttribute propertyGroupDescription)
        {
            string path = Path + ".";

            if (!typeof(LayerPropertyGroup).IsAssignableFrom(propertyInfo.PropertyType))
                throw new ArtemisPluginException("Layer property with PropertyGroupDescription attribute must be of type LayerPropertyGroup");

            if (!(Activator.CreateInstance(propertyInfo.PropertyType) is LayerPropertyGroup instance))
                throw new ArtemisPluginException($"Failed to create instance of layer property group at {path + propertyInfo.Name}");

            // Ensure the description has a name, if not this is a good point to set it based on the property info
            if (string.IsNullOrWhiteSpace(propertyGroupDescription.Name))
                propertyGroupDescription.Name = propertyInfo.Name.Humanize();

            instance.Parent = this;
            instance.GroupDescription = propertyGroupDescription;
            instance.LayerBrush = LayerBrush;
            instance.LayerEffect = LayerEffect;
            instance.Initialize(ProfileElement, $"{path}{propertyInfo.Name}.", Feature);

            propertyInfo.SetValue(this, instance);
            _layerPropertyGroups.Add(instance);
        }

        private PropertyEntity GetPropertyEntity(RenderProfileElement profileElement, string path, out bool fromStorage)
        {
            PropertyEntity? entity = profileElement.RenderElementEntity.PropertyEntities.FirstOrDefault(p => p.FeatureId == Feature.Id && p.Path == path);
            fromStorage = entity != null;
            if (entity == null)
            {
                entity = new PropertyEntity {FeatureId = Feature.Id, Path = path};
                profileElement.RenderElementEntity.PropertyEntities.Add(entity);
            }

            return entity;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}